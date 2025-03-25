using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace PlayerScaling;

[BepInPlugin(modGUID, modName, modVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string modGUID = "dev.redfops.repo.playerscaling";
    public const string modName = "playerscaling";
    public const string modVersion = "1.0.1";

    internal static new ManualLogSource Logger;
    private readonly Harmony harmony = new Harmony(modGUID);

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        harmony.PatchAll(typeof(TileGenerationPatchTrans));
        harmony.PatchAll(typeof(TileGenerationPatch));
        harmony.PatchAll(typeof(DifficultyPatch));
        harmony.PatchAll(typeof(EnemyAmountPatch));
        harmony.PatchAll(typeof(ValuablePatchTrans));
        harmony.PatchAll(typeof(ValuablePatch));
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

}



//Larger levels as we get more players
[HarmonyPatch(typeof(LevelGenerator))]
[HarmonyPatch("TileGeneration")]
public static class TileGenerationPatch
{
    static void Prefix(LevelGenerator __instance, ref int ___ModuleAmount, ref int ___ExtractionAmount)
    {
        int players = GameDirector.instance.PlayerList.Count;
        //players = 24; //Testing

        ___ExtractionAmount = 0;

        //Scaling only kicks in after 4
        if(players <= 4)
        {
            //Set to default values
            if (___ModuleAmount > 4)
            {
                ___ModuleAmount = Mathf.Min(5 + RunManager.instance.levelsCompleted, 10);
                if (___ModuleAmount >= 10)
                {
                    ___ExtractionAmount = 3;
                }
                else if (___ModuleAmount >= 8)
                {
                    ___ExtractionAmount = 2;
                }
                else if (___ModuleAmount >= 6)
                {
                    ___ExtractionAmount = 1;
                }
                else
                {
                    ___ExtractionAmount = 0;
                }
            }
            __instance.LevelHeight = 3;
            __instance.LevelWidth = 3;
            return;
        }

        float playerScalingAmount = players / 4f;
        __instance.LevelHeight = (int)(3f * Mathf.Sqrt(playerScalingAmount + 2));
        __instance.LevelWidth = __instance.LevelHeight;
        if (___ModuleAmount > 4)
        {
            ___ModuleAmount = Mathf.Min((int)(playerScalingAmount * (5 + RunManager.instance.levelsCompleted)), __instance.LevelHeight * __instance.LevelWidth);
            ___ExtractionAmount = (___ModuleAmount - 4) / 2;
        }
    }
#if DEBUG
    static void Postfix(LevelGenerator __instance, ref int ___ModuleAmount, ref int ___ExtractionAmount)
    {
        Plugin.Logger.LogInfo(string.Format("LevelGeneration Completed, ModuleAmount: {0,10}, LevelHeight: {1,10}, LevelWidth: {2,10}", ___ModuleAmount, __instance.LevelHeight, __instance.LevelWidth));
        Plugin.Logger.LogInfo(string.Format("ExtractionAmount: {0,10}", ___ExtractionAmount));
    }
#endif
}

//The transpiler method is different because reasons ig
[HarmonyPatch(typeof(LevelGenerator),"TileGeneration", MethodType.Enumerator)]
public static class TileGenerationPatchTrans
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var startIndex = -1;
        var endIndex = -1;

        var codes = new List<CodeInstruction>(instructions);

        //Remove the following snippet:
        /* ModuleAmount = Mathf.Min(5 + RunManager.instance.levelsCompleted, 10);
		ModuleAmount = Mathf.CeilToInt((float)ModuleAmount * DebugLevelSize);*/

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].StoresField(typeof(LevelGenerator).GetField("ModuleAmount", BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                if (!found)
                {
                    found = true;
                    for (int j = i; j > 0; j--)
                    {
                        if (codes[j].opcode == OpCodes.Ble)
                        {
                            startIndex = j + 1;
                            break;
                        }
                    }
                } else
                {
                    endIndex = i + 1;
                    break;
                }
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            codes.RemoveRange(startIndex, endIndex - startIndex);
        }
        else
        {
            Plugin.Logger.LogError("Cannot find <Stdfld ModuleAmount> in LevelGenerator.TileGeneration");
        }
        startIndex = -1;
        endIndex = -1;

        //Remove line: ExtractionAmount = 0;

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].StoresField(typeof(LevelGenerator).GetField("ExtractionAmount", BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                if (codes[i - 1].opcode == OpCodes.Ldc_I4_0)
                {
                    startIndex = i - 2;
                    endIndex = i + 1;
                    break;
                }
                continue;
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            codes.RemoveRange(startIndex, endIndex - startIndex);
        }
        else
        {
            Plugin.Logger.LogError("Cannot find <Stdfld ExtractionAmount> in LevelGenerator.TileGeneration");
        }

        found = false;
        startIndex = -1;
        endIndex = -1;

        //Remove the following snippet:
        /* if (ModuleAmount >= 10)
			{
				ExtractionAmount = 3;
			}
			else if (ModuleAmount >= 8)
			{
				ExtractionAmount = 2;
			}
			else if (ModuleAmount >= 6)
			{
				ExtractionAmount = 1;
			}
			else
			{
				ExtractionAmount = 0;
			}*/

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].LoadsField(typeof(LevelGenerator).GetField("ModuleAmount", BindingFlags.NonPublic | BindingFlags.Instance)) && !found)
            {
                if (codes[i+1].opcode == OpCodes.Ldc_I4_S && Convert.ToInt32(codes[i+1].operand) == 10 && codes[i+2].opcode == OpCodes.Blt)
                {
                    found = true;
                    startIndex = i - 1;
                }
                continue;
            }

            if (found && codes[i].StoresField(typeof(LevelGenerator).GetField("ExtractionAmount", BindingFlags.NonPublic | BindingFlags.Instance))){
                if (codes[i-1].opcode == OpCodes.Ldc_I4_0)
                {
                    endIndex = i + 1;
                    break;
                }
                continue;
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            codes.RemoveRange(startIndex, endIndex - startIndex);
        }
        else
        {
            Plugin.Logger.LogError("Cannot find <Stdfld ExtractionAmount> in LevelGenerator.TileGeneration");
        }

        return codes.AsEnumerable();
    }
}

[HarmonyPatch(typeof(SemiFunc))]
[HarmonyPatch(nameof(SemiFunc.RunGetDifficultyMultiplier))]
public static class DifficultyPatch
{
    static void Postfix(ref float __result)
    {
        //Have game play as default for 4 players or less, except when debugging
#if !DEBUG
        if (GameDirector.instance.PlayerList.Count <= 4) {
            return;
        }
#endif
        float playerScalingAmount = GameDirector.instance.PlayerList.Count / 4f;
        //playerScalingAmount = 4; //Testing
        __result += 0.085f; //Slight bump to make playerScaling actually kick in on first round
        __result *= Mathf.Sqrt(playerScalingAmount);
    }
}

[HarmonyPatch(typeof(EnemyDirector))]
[HarmonyPatch(nameof(EnemyDirector.AmountSetup))]
//Enemy related patches
//The curves cap the difficulty so the solution is to add the multiplier post-curve
public static class EnemyAmountPatch
{
    static void Prefix(ref int ___amountCurve1Value, ref int ___amountCurve2Value, ref int ___amountCurve3Value, EnemyDirector __instance)
    {
        float playerScalingAmount = GameDirector.instance.PlayerList.Count / 4f;
        //playerScalingAmount = 4; //Testing
        float difficultyUnscaled = SemiFunc.RunGetDifficultyMultiplier() / Mathf.Sqrt(playerScalingAmount);

        //Have game play as default for 4 players or less, except when debugging
#if !DEBUG
        if (GameDirector.instance.PlayerList.Count <= 4)
        {
            //Since we can't change our transpiler patch at runtime we have to calculate these values as the function would have without the patch as a prefix
            difficultyUnscaled = SemiFunc.RunGetDifficultyMultiplier();
            playerScalingAmount = 1;
        }
#endif

        ___amountCurve1Value = (int)(__instance.amountCurve1.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___amountCurve2Value = (int)(__instance.amountCurve2.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___amountCurve3Value = (int)(__instance.amountCurve3.Evaluate(difficultyUnscaled) * playerScalingAmount);
    }
#if DEBUG
    static void Postfix(ref int ___amountCurve1Value, ref int ___amountCurve2Value, ref int ___amountCurve3Value)
    {
        Plugin.Logger.LogInfo(string.Format("Tier 1 enemies: {0,10}, Tier 2 enemies: {1,10}, Tier 3 enemies: {2,10}", ___amountCurve1Value, ___amountCurve2Value, ___amountCurve3Value));
    }
#endif
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var endIndex = -1;

        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].StoresField(typeof(EnemyDirector).GetField("amountCurve1Value", BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                endIndex = i + 1;
            }
        }

        if (endIndex > -1)
        {
            codes.RemoveRange(0, endIndex);
        }
        else
        {
            Plugin.Logger.LogError("Cannot find <Stdfld amountCurve1Value> in EnemyDirector.AmountSetup");
        }
        return codes.AsEnumerable();
    }
}

[HarmonyPatch(typeof(ValuableDirector))]
[HarmonyPatch(nameof(ValuableDirector.SetupHost))]
public static class ValuablePatch
{
    static void Prefix(ref int ___totalMaxAmount, ref int ___tinyMaxAmount, ref int ___smallMaxAmount, ref int ___mediumMaxAmount, ref int ___bigMaxAmount, ref int ___wideMaxAmount, ref int ___tallMaxAmount, ref int ___veryTallMaxAmount, ValuableDirector __instance)
    {

        float playerScalingAmount = GameDirector.instance.PlayerList.Count / 4f;
        float difficultyUnscaled = (SemiFunc.RunGetDifficultyMultiplier() / Mathf.Sqrt(playerScalingAmount)) + 0.1f; //Little extra bump

        //Have game play as default for 4 players or less, except when debugging
#if !DEBUG
        if (GameDirector.instance.PlayerList.Count <= 4)
        {
            //Since we can't change our transpiler patch at runtime we have to calculate these values as the function would have without the patch as a prefix
            difficultyUnscaled = SemiFunc.RunGetDifficultyMultiplier();
            playerScalingAmount = 1;
        }
#endif
#if DEBUG
        Plugin.Logger.LogInfo(string.Format("Valuable Volumes: {0,10}", UnityEngine.Object.FindObjectsOfType<ValuableVolume>(includeInactive: false).ToList().Count));
#endif
        ___totalMaxAmount = Mathf.RoundToInt(__instance.totalMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___tinyMaxAmount = Mathf.RoundToInt(__instance.tinyMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___smallMaxAmount = Mathf.RoundToInt(__instance.smallMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___mediumMaxAmount = Mathf.RoundToInt(__instance.mediumMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___bigMaxAmount = Mathf.RoundToInt(__instance.bigMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___wideMaxAmount = Mathf.RoundToInt(__instance.wideMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___tallMaxAmount = Mathf.RoundToInt(__instance.tallMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
        ___veryTallMaxAmount = Mathf.RoundToInt(__instance.veryTallMaxAmountCurve.Evaluate(difficultyUnscaled) * playerScalingAmount);
    }
#if DEBUG
    static void Postfix(ref int ___totalMaxAmount, ref int ___tinyMaxAmount, ref int ___smallMaxAmount, ref int ___mediumMaxAmount, ref int ___bigMaxAmount, ref int ___wideMaxAmount, ref int ___tallMaxAmount, ref int ___veryTallMaxAmount, ValuableDirector __instance)
    {
        Plugin.Logger.LogInfo(string.Format("totalmax: {0,10}, tinymax: {1,10}, smallmax: {2,10}", ___totalMaxAmount, ___tinyMaxAmount, ___smallMaxAmount));
        Plugin.Logger.LogInfo(string.Format("mediummax: {0,10}, bigmax: {1,10}, widemax: {2,10}", ___mediumMaxAmount, ___bigMaxAmount, ___wideMaxAmount));
        Plugin.Logger.LogInfo(string.Format("tallmax: {0,10}, veryTallmax: {1,10}", ___tallMaxAmount, ___veryTallMaxAmount));
    }
#endif
}

//IEnumerator creates a weird seperate function for the sake of transpiling
[HarmonyPatch(typeof(ValuableDirector), nameof(ValuableDirector.SetupHost), MethodType.Enumerator)]
public static class ValuablePatchTrans
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var startIndex = -1;
        var endIndex = -1;

        Label label = generator.DefineLabel();

        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            
            if (codes[i].StoresField(typeof(ValuableDirector).GetField("totalMaxAmount", BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                for (int j = i; j > 0; j--)
                {
                    if (codes[j].opcode == OpCodes.Ldc_R4)
                    {
                        startIndex = j + 2;
                    }
                    if (codes[j].opcode == OpCodes.Brfalse)
                    {
                        codes[j].operand = label;
                        break;
                    }
                }
                continue;
            }

            if (codes[i].StoresField(typeof(ValuableDirector).GetField("veryTallMaxAmount", BindingFlags.NonPublic | BindingFlags.Instance)))
            {
                endIndex = i + 1;
                codes[i + 1].labels.Add(label);

                break;
            }
        }

        if (startIndex > -1 && endIndex > -1)
        {
            codes.RemoveRange(startIndex, endIndex - startIndex);
        }
        else
        {
            Plugin.Logger.LogError("Cannot find <Stdfld totalMaxAmount> in ValuableDirector.SetupHost");
        }


        return codes.AsEnumerable();
    }
}
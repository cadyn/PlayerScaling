using HarmonyLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using static UnityEngine.Rendering.VolumeComponent;

namespace PlayerScaling
{

    //Larger levels as we get more players
    [HarmonyPatch(typeof(LevelGenerator))]
    [HarmonyPatch("TileGeneration")]
    public static class TileGenerationPatch
    {
        static void Prefix(LevelGenerator __instance, ref int ___ModuleAmount, ref int ___ExtractionAmount)
        {
            float playerScalingAmount = Plugin.PlayerScaling(ScalingType.Map);

            ___ExtractionAmount = 0;
            if (___ModuleAmount > 4)
            {
                int maxMapSize = Mathf.Min(Mathf.CeilToInt(Plugin.defaultMaxMapSize.Value * playerScalingAmount),2000); //Capped at 2000 for sanity sake
                if (maxMapSize > 200)
                {
                    __instance.LevelHeight = 50;
                    __instance.LevelWidth = 50;
                }
                ___ModuleAmount = Mathf.Min((int)(playerScalingAmount * (5 + RunManager.instance.levelsCompleted)), maxMapSize);
                Plugin.curModuleAmount = ___ModuleAmount;
                ___ExtractionAmount = (___ModuleAmount - 4) / 2;
            }
        }
#if DEBUG
    static void Postfix(LevelGenerator __instance, ref int ___ModuleAmount, ref int ___ExtractionAmount, ref int ___DeadEndAmount, ref GameObject ___DebugModule)
    {
        Plugin.Logger.LogInfo(string.Format("playerScalingAmount: {0,10}", Plugin.PlayerScaling(ScalingType.Map)));
        Plugin.Logger.LogInfo(string.Format("ModuleAmount: {0,10}, LevelHeight: {1,10}, LevelWidth: {2,10}", ___ModuleAmount, __instance.LevelHeight, __instance.LevelWidth));
        Plugin.Logger.LogInfo(string.Format("ExtractionAmount: {0,10}, DeadEndAmount: {1,10}, DebugModule: " + (___DebugModule ? "True" : "False"), ___ExtractionAmount, ___DeadEndAmount));
    }
#endif
    }

    //The transpiler method is different because reasons ig
    [HarmonyPatch(typeof(LevelGenerator), "TileGeneration", MethodType.Enumerator)]
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
                    }
                    else
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
                    if (codes[i + 1].opcode == OpCodes.Ldc_I4_S && Convert.ToInt32(codes[i + 1].operand) == 10 && codes[i + 2].opcode == OpCodes.Blt)
                    {
                        found = true;
                        startIndex = i - 1;
                    }
                    continue;
                }

                if (found && codes[i].StoresField(typeof(LevelGenerator).GetField("ExtractionAmount", BindingFlags.NonPublic | BindingFlags.Instance)))
                {
                    if (codes[i - 1].opcode == OpCodes.Ldc_I4_0)
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
}

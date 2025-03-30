using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlayerScaling
{
    [HarmonyPatch(typeof(ValuableDirector))]
    [HarmonyPatch(nameof(ValuableDirector.SetupHost))]
    public static class ValuablePatch
    {
        static void Prefix(ref int ___totalMaxAmount, ref int ___tinyMaxAmount, ref int ___smallMaxAmount, ref int ___mediumMaxAmount, ref int ___bigMaxAmount, ref int ___wideMaxAmount, ref int ___tallMaxAmount, ref int ___veryTallMaxAmount, ValuableDirector __instance)
        {

            float playerScalingAmount = Plugin.PlayerScaling(ScalingType.Valuable);
            float difficultyUnscaled = (SemiFunc.RunGetDifficultyMultiplier() / Mathf.Sqrt(Plugin.PlayerScaling(ScalingType.Difficulty))) + Plugin.valuableScalingOffset.Value; //Little extra bump

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
}

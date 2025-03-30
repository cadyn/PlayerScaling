using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlayerScaling
{
    [HarmonyPatch(typeof(EnemyDirector))]
    [HarmonyPatch(nameof(EnemyDirector.AmountSetup))]
    //Enemy related patches
    //The curves cap the difficulty so the solution is to add the multiplier post-curve
    public static class EnemyAmountPatch
    {
        static void Prefix(ref int ___amountCurve1Value, ref int ___amountCurve2Value, ref int ___amountCurve3Value, EnemyDirector __instance)
        {
            float playerScalingAmount = Plugin.PlayerScaling(ScalingType.Enemy);
            float difficultyUnscaled = SemiFunc.RunGetDifficultyMultiplier() / Mathf.Sqrt(Plugin.PlayerScaling(ScalingType.Difficulty));

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
}

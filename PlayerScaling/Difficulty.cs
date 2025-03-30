using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlayerScaling
{
    [HarmonyPatch(typeof(SemiFunc))]
    [HarmonyPatch(nameof(SemiFunc.RunGetDifficultyMultiplier))]
    public static class DifficultyPatch
    {
        static void Postfix(ref float __result)
        {
            float playerScalingAmount = Plugin.PlayerScaling(ScalingType.Difficulty);
            if (playerScalingAmount == 1) return;
            __result += Plugin.difficultyScalingOffset.Value; //Slight bump to make playerScaling actually kick in on first round
            __result *= Mathf.Sqrt(playerScalingAmount);
        }
    }
}

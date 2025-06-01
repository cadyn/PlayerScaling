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
        private static Traverse TraverseCheckMaxAmount;
        private static Traverse TraverseCheckMaxValue;
        private static AnimationCurve TotalMaxAmountDefault;
        private static AnimationCurve TotalMaxValueDefault;
        private static AnimationCurve TinyDefault;
        private static AnimationCurve SmallDefault;
        private static AnimationCurve MediumDefault ;
        private static AnimationCurve BigDefault;
        private static AnimationCurve WideDefault;
        private static AnimationCurve TallDefault;
        private static AnimationCurve VeryTallDefault;
        
        static void Prefix(ref int ___totalMaxAmount, ValuableDirector __instance)
        {
            Plugin.Logger.LogInfo("Player scaling runs HERE");
            var TraverseCheckMaxAmount = Traverse.Create(__instance).Field("totalMaxAmountCurve");
            var TraverseCheckMaxValue = Traverse.Create(__instance).Field("totalMaxValueCurve");
            
            int vanillaMapSize = Mathf.Min(10, 5 + RunManager.instance.levelsCompleted);

            //Similar to enemies, we try to maintain density, but I'm not fucking around with trying to replicate these curves.
            float mapScalingFactor = Plugin.curModuleAmount / (float)vanillaMapSize;
            mapScalingFactor *= Plugin.valuableScalingMultiplier.Value;

            if(TraverseCheckMaxAmount.FieldExists())
                TraverseCheckMaxAmount.SetValue(ReplaceCurve(ref TotalMaxAmountDefault, TraverseCheckMaxAmount.GetValue<AnimationCurve>(), value => value * mapScalingFactor));
            else // in beta it's a hard coded value that never changes so just replace it -- this is also what breaks the transpiler as it looks for an instruction setting it
                ___totalMaxAmount = (int) Math.Ceiling(___totalMaxAmount * mapScalingFactor);
            
            if(TraverseCheckMaxValue.FieldExists()) // this only exists in beta
                TraverseCheckMaxValue.SetValue(ReplaceCurve(ref TotalMaxValueDefault, TraverseCheckMaxValue.GetValue<AnimationCurve>(), value => value * mapScalingFactor));
            
            __instance.tinyMaxAmountCurve = ReplaceCurve(ref TinyDefault, __instance.tinyMaxAmountCurve, value => value * mapScalingFactor);
            __instance.smallMaxAmountCurve = ReplaceCurve(ref SmallDefault, __instance.smallMaxAmountCurve, value => value * mapScalingFactor);
            __instance.mediumMaxAmountCurve = ReplaceCurve(ref MediumDefault, __instance.mediumMaxAmountCurve, value => value * mapScalingFactor);
            __instance.bigMaxAmountCurve = ReplaceCurve(ref BigDefault, __instance.bigMaxAmountCurve, value => value * mapScalingFactor);
            __instance.wideMaxAmountCurve = ReplaceCurve(ref WideDefault, __instance.wideMaxAmountCurve, value => value * mapScalingFactor);
            __instance.tallMaxAmountCurve = ReplaceCurve(ref TallDefault, __instance.tallMaxAmountCurve, value => value * mapScalingFactor);
            __instance.veryTallMaxAmountCurve = ReplaceCurve(ref VeryTallDefault, __instance.veryTallMaxAmountCurve, value => value * mapScalingFactor);
        }
        
#if DEBUG
        static void Postfix(ref int ___totalMaxAmount, ref int ___tinyMaxAmount, ref int ___smallMaxAmount, ref int ___mediumMaxAmount, ref int ___bigMaxAmount, ref int ___wideMaxAmount, ref int ___tallMaxAmount, ref int ___veryTallMaxAmount, ValuableDirector __instance)
        {
            Plugin.Logger.LogInfo(string.Format("totalmax: {0,10}, tinymax: {1,10}, smallmax: {2,10}", ___totalMaxAmount, ___tinyMaxAmount, ___smallMaxAmount));
            Plugin.Logger.LogInfo(string.Format("mediummax: {0,10}, bigmax: {1,10}, widemax: {2,10}", ___mediumMaxAmount, ___bigMaxAmount, ___wideMaxAmount));
            Plugin.Logger.LogInfo(string.Format("tallmax: {0,10}, veryTallmax: {1,10}", ___tallMaxAmount, ___veryTallMaxAmount));
        }
#endif
        
        private static AnimationCurve ReplaceCurve(ref AnimationCurve target, AnimationCurve source, Func<float, float> calculate) {
            // the compiler will warn of unintended reference comparison; it is completely intended
            if (target == source) return target;
            
            target = new AnimationCurve();
            
            target.CopyFrom(source); // duplicate curve parameters
            target.ClearKeys(); // but replace with the scaled keyframes
            
            foreach (var key in source.GetKeys()) {
                var newKey = key with { value = calculate.Invoke(key.value)};
                target.AddKey(newKey);
            }

            return target;
        }
    }
}

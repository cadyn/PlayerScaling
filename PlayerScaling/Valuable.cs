using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace PlayerScaling
{
    [HarmonyPatch(typeof(ValuableDirector))]
    [HarmonyPatch(nameof(ValuableDirector.SetupHost))]
    [HarmonyWrapSafe]
    public static class ValuablePatch
    {
        private static AnimationCurve[] TotalMaxAmountCurves { get; set; }
        private static AnimationCurve[] TotalMaxValueCurves { get; set; }
        private static AnimationCurve[] TinyCurves { get; set; }
        private static AnimationCurve[] SmallCurves { get; set; }
        private static AnimationCurve[] MediumCurves { get;  set; }
        private static AnimationCurve[] BigCurves { get; set; }
        private static AnimationCurve[] WideCurves { get; set; }
        private static AnimationCurve[] TallCurves { get; set; }
        private static AnimationCurve[] VeryTallCurves { get; set; }
        
        static void Prefix(ValuableDirector __instance)
        {
            if(__instance == null) return;
            var totalMaxAmountRef = FieldRefAccess<ValuableDirector, int>("totalMaxAmount");
            Plugin.Logger.LogInfo("Player scaling runs HERE");
            var difficultyDelegates = GetStaticNumberedMethodDelegates<Plugin.DifficultyDelegate>(typeof(SemiFunc), "RunGetDifficultyMultiplier", [], 10);
            
            int vanillaMapSize = Mathf.Min(10, 5 + RunManager.instance.levelsCompleted);
            if (RunManager.instance.levelsCompleted >= 10)
                vanillaMapSize += Mathf.Min(RunManager.instance.levelsCompleted - 9, 5);

            //Similar to enemies, we try to maintain density, but I'm not fucking around with trying to replicate these curves.
            float mapScalingFactor = Plugin.curModuleAmount / (float)vanillaMapSize;
            mapScalingFactor *= Plugin.valuableScalingMultiplier.Value;

            var maxAmountFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "totalMaxAmountCurve", 10);
            if(maxAmountFieldRefs.Count != 0) {
                TotalMaxAmountCurves ??= new AnimationCurve[maxAmountFieldRefs.Count];
                foreach (var curve in maxAmountFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                        .Where(tuple => tuple.Item1 != null && tuple.Item2 != null)
                                                        .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))) 
                    curve.Item2.Invoke(__instance) = ReplaceCurve(ref TotalMaxAmountCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            }
            else // in beta it's a hard coded value that never changes so just replace it -- this is also what breaks the transpiler as it looks for an instruction setting it
            {
                totalMaxAmountRef.Invoke(__instance) = (int)Math.Ceiling(totalMaxAmountRef.Invoke(__instance) * mapScalingFactor);
            }

            var maxValueFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "totalMaxValueCurve", 10);
            if(maxValueFieldRefs.Count != 0) // this only exists in beta
            {
                TotalMaxValueCurves ??= new AnimationCurve[maxValueFieldRefs.Count];
                foreach (var curve in maxValueFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                       .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                       .Where(value => value.Item2 != null && value.Item3 != null)) 
                    curve.Item2.Invoke(__instance) = ReplaceCurve(ref TotalMaxValueCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            }
            
            var tinyMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "tinyMaxAmountCurve", 10);
            TinyCurves ??= new AnimationCurve[tinyMaxFieldRefs.Count];
            foreach (var curve in tinyMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                   .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref TinyCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var smallMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "smallMaxAmountCurve", 10);
            SmallCurves ??= new AnimationCurve[smallMaxFieldRefs.Count];
            foreach (var curve in smallMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref SmallCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var mediumMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "mediumMaxAmountCurve", 10);
            MediumCurves ??= new AnimationCurve[mediumMaxFieldRefs.Count];
            foreach (var curve in mediumMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref MediumCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var bigMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "bigMaxAmountCurve", 10);
            BigCurves ??= new AnimationCurve[bigMaxFieldRefs.Count];
            foreach (var curve in bigMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref BigCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var wideMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "wideMaxAmountCurve", 10);
            WideCurves ??= new AnimationCurve[wideMaxFieldRefs.Count];
            foreach (var curve in wideMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref WideCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var tallMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "tallMaxAmountCurve", 10);
            TallCurves ??= new AnimationCurve[tallMaxFieldRefs.Count];
            foreach (var curve in tallMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref TallCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
            var veryTallMaxFieldRefs = GetNumberedFieldRefs<ValuableDirector, AnimationCurve>(__instance, "veryTallMaxAmountCurve", 10);
            VeryTallCurves ??= new AnimationCurve[veryTallMaxFieldRefs.Count];
            foreach (var curve in veryTallMaxFieldRefs.Zip(difficultyDelegates, Tuple.Create)
                                                  .Select((value, index) => Tuple.Create(index, value.Item1, value.Item2))
                                                  .Where(value => value.Item2 != null && value.Item3 != null)) 
                curve.Item2.Invoke(__instance) = ReplaceCurve(ref VeryTallCurves[curve.Item1], curve.Item2.Invoke(__instance), value => value * mapScalingFactor);
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
            Plugin.Logger.LogInfo("Player scaling will replace a curve with a factor of " + calculate(1f));
            if (target == source) return target;
            
            target = new AnimationCurve();
            
            Plugin.Logger.LogInfo("Player scaling will copy a curve for scaling");
            target.CopyFrom(source); // duplicate curve parameters
            target.ClearKeys(); // but replace with the scaled keyframes
            
            foreach (var key in source.GetKeys()) 
            {
                var newKey = key with { value = calculate.Invoke(key.value)};
                target.AddKey(newKey);
            }

            return target;
        }
        
        private static List<FieldRef<S, T>> GetNumberedFieldRefs<S, T>(S source, string expectedBaseName, int checkMax, int checkMin = 0) 
        {
            var fields = new List<FieldRef<S, T>>();

            if (Traverse.Create(source).Field(expectedBaseName).FieldExists()) {
                fields.Add(FieldRefAccess<S, T>(expectedBaseName));
            }

            for (var i = checkMin; i < checkMax; i++) {
                if (Traverse.Create(source).Field(expectedBaseName + i).FieldExists()) {
                    fields.Add(FieldRefAccess<S, T>(expectedBaseName + i));
                }
            }
        
            return fields;
        }
        
        private static List<T> GetStaticNumberedMethodDelegates<T>(Type source, string expectedBaseName, Type[] parameters, int checkMax, int checkMin = 0) where T : Delegate 
        {
            var methods = new List<T>();
        
            if (Traverse.Create(source).Method(expectedBaseName, parameters).MethodExists()) 
            {
                methods.Add(MethodDelegate<T>(Method(source, expectedBaseName, parameters), source, true));
            }
        
            for (var i = checkMin; i < checkMax; i++) 
            {
                if (Traverse.Create(source).Method(expectedBaseName + i, parameters).MethodExists()) 
                {
                    methods.Add(MethodDelegate<T>(Method(source, expectedBaseName + i, parameters), source, true));
                }
            }
        
            return methods;
        }
    }
}

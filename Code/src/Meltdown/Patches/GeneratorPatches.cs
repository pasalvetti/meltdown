using HarmonyLib;
using KSP.Iteration.UI.Binding;
using KSP.Modules;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;

namespace Meltdown.Patches
{
    internal class GeneratorPatches
    {
        /**
         * Marks the generator as a heat-generating part and enable heat generation for generators.
         **/
        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.ThermalUpdate))]
        [HarmonyPostfix]
        public static void ThermalUpdatePostFix(Module_Generator __instance)
        {
            bool isHeating = __instance.dataGenerator.GeneratorIsActive;
            bool isEngine = __instance.part.TryGetComponent<Module_Engine>(out _);
            double rate = 0.0;
            if (isEngine)
            {
                if (__instance.EngineModule != null) // for cornet, trumpet and tuba engines
                {
                    rate = (double)__instance.EngineModule.throttleSetting;
                }
                if (__instance._engineStatus != null) // for other engines
                {
                    rate = (double)__instance._engineStatus.normalizedOutput;
                }
            } else
            {
                rate = 1.0;
            }
            double fluxGenerated = MeltdownPlugin.GenerateFlux(__instance._componentModule, isHeating, rate, usePatchedFlux: true);
            __instance.dataGenerator.FluxGenerated = fluxGenerated;
        }
    }
}

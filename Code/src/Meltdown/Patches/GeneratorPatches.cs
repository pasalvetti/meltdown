using HarmonyLib;
using KSP.Modules;

namespace Meltdown.Patches
{
    internal class GeneratorPatches
    {
        /**
         * Prefix so that the field gets displayed in SetPAMVisibility().
         **/
        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.OnInitialize))]
        [HarmonyPrefix]
        static public void OnInitializePreFix(Module_Generator __instance) // tourne
        {
            __instance.dataGenerator.AutoShutdown = true; // utile ?
        }

        /**
         * Marks the generator as a heat-generating part and enable heat generation for generators.
         **/
        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.ThermalUpdate))]
        [HarmonyPrefix]
        public static void ThermalUpdatePostFix(double universalTime, Module_Generator __instance)
        {
            bool isHeating = __instance.dataGenerator.GeneratorIsActive;
            double rate = __instance._engineStatus == null ? 1.0 : (double)__instance._engineStatus.normalizedOutput;
            double fluxGenerated = MeltdownPlugin.GenerateFlux(__instance._componentModule, isHeating, rate, usePatchedFlux: true);
            __instance.dataGenerator.FluxGenerated = fluxGenerated;
        }
    }
}

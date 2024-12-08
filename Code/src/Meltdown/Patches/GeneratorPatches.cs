using HarmonyLib;
using KSP.Modules;
using KSP.Sim.impl;

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
        [HarmonyPatch(typeof(PartComponentModule_Generator), nameof(PartComponentModule_Generator.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdatePreFix(double universalTime, PartComponentModule_Generator __instance)
        {
            double fluxGenerated = MeltdownPlugin.GenerateFlux(__instance, isHeating: true, rate: 1.0, usePatchedFlux: false);
            __instance.dataGenerator.FluxGenerated = fluxGenerated;
        }
    }
}

using HarmonyLib;
using KSP.Iteration.UI.Binding;
using KSP.Modules;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;

namespace Meltdown.Patches
{
    internal class GeneratorPatches
    {
        /**
         * Enables autoshutdown. Useful?
         **/
        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.OnInitialize))]
        [HarmonyPrefix]
        static public void OnInitializePreFix(Module_Generator __instance)
        {
            __instance.dataGenerator.AutoShutdown = true;
        }

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
                //System.Diagnostics.Debug.Write("[Meltdown] Module_Generator.ThermalUpdate: " + __instance.part.Model.Name + " " + __instance.part.Model.GlobalId + "/EngineModule=" + __instance.EngineModule);
                if (__instance.EngineModule != null) // for cornet, trumpet and tuba engines
                {
                    //System.Diagnostics.Debug.Write("[Meltdown] Module_Generator.ThermalUpdate: " + __instance.part.Model.Name + " " + __instance.part.Model.GlobalId + "/throttleSetting=" + __instance.EngineModule.throttleSetting);
                    rate = (double)__instance.EngineModule.throttleSetting;
                }
                //System.Diagnostics.Debug.Write("[Meltdown] Module_Generator.ThermalUpdate: " + __instance.part.Model.Name + " " + __instance.part.Model.GlobalId + "/_engineStatus=" + __instance._engineStatus);
                if (__instance._engineStatus != null) // for other engines
                {
                    rate = (double)__instance._engineStatus.normalizedOutput;
                    //System.Diagnostics.Debug.Write("[Meltdown] Module_Generator.ThermalUpdate: " + __instance.part.Model.Name + " " + __instance.part.Model.GlobalId + "/normalizedOutput=" + __instance._engineStatus.normalizedOutput);
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

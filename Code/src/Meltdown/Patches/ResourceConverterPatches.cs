using HarmonyLib;
using KSP.Modules;
using KSP.Sim.impl;

namespace Meltdown.Patches
{
    internal class ResourceConverterPatches
    {
        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnToggleChangedValue))]
        [HarmonyPostfix]
        public static void OnToggleChangedValuePostFix(Module_ResourceConverter __instance) // fonctionne
        {
            __instance._dataResourceConverter.ConverterIsActive = !__instance._dataResourceConverter.ConverterIsActive; // nécessaire pour faire augmenter la température quand on active le générateur
        }

        /**
         * Generates heat.
         **/
        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.ThermalUpdate))]
        [HarmonyPostfix]
        public static void ThermalUpdatePostFix(double deltaTime, Module_ResourceConverter __instance)
        {
            /* Fixes stock errors:
             * - the flux should be multiplied by the conversion rate
             * - the flux should not be multiplied by the time */
            /* Marks as heating if the converter is on and has a rate > 0 and has enough resources */
            bool isHeating = __instance._dataResourceConverter.ConverterIsActive && __instance._dataResourceConverter.conversionRate.GetValue() > 0 && __instance.HasEnoughResources();
            double rate = __instance._dataResourceConverter.conversionRate.GetValue();
            double fluxGenerated = MeltdownPlugin.GenerateFlux(__instance._componentModule, isHeating, rate, usePatchedFlux: true);
            __instance._dataResourceConverter.FluxGenerated = fluxGenerated;
        }

        /**
         * Stores the value of ConverterIsActive in __state for use in OnUpdatePostFix.
         **/
        [HarmonyPatch(typeof(PartComponentModule_ResourceConverter), nameof(PartComponentModule_ResourceConverter.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdatePreFix(PartComponentModule_ResourceConverter __instance, ref bool __state)
        {
            __state = __instance._dataResourceConverter.ConverterIsActive;
        }

        /**
         * OnUpdate seems to randomly change the value of ConverterIsActive from one frame to another, which is messing with heat generation in Module_ResourceConverter.ThermalUpdate().
         * This assigns the value it had in Prefix.
         **/
        [HarmonyPatch(typeof(PartComponentModule_ResourceConverter), nameof(PartComponentModule_ResourceConverter.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(PartComponentModule_ResourceConverter __instance, bool __state)
        {
            if (!__instance._dataResourceConverter.EnabledToggle.GetValue()) return;
            __instance._dataResourceConverter.ConverterIsActive = __state;
        }

    }
}

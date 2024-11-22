using HarmonyLib;
using KSP.Api;
using KSP.Modules;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;

namespace Meltdown
{
    internal class MeltdownPlugin
    {

        /** Module_Generator **/

        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_Generator __instance) // tourne
        {
            //System.Diagnostics.Debug.Write("Meltdown: OnInitializePostFix");
            __instance.dataGenerator.AutoShutdown = true; // utile ?
            __instance.dataGenerator.FluxGenerated = 30000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
            //System.Diagnostics.Debug.Write("--FluxGenerated=" + __instance.dataGenerator.FluxGenerated); // ok
        }

        /** Resource_converter **/

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_ResourceConverter __instance) // tourne
        {
            //System.Diagnostics.Debug.Write("Meltdown: Module_ResourceConverter.OnInitializePostFix");
            __instance._dataResourceConverter.FluxGenerated = 30000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
            //System.Diagnostics.Debug.Write("--FluxGenerated=" + __instance._dataResourceConverter.FluxGenerated); // ok
        }

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnToggleChangedValue))]
        [HarmonyPostfix]
        public static void OnToggleChangedValuePostFix(Module_ResourceConverter __instance) // fonctionne
        {
            System.Diagnostics.Debug.Write("Meltdown: OnToggleChangedValuePostFix");
            __instance._dataResourceConverter.ConverterIsActive = !__instance._dataResourceConverter.ConverterIsActive; // nécessaire pour faire augmenter la température quand on active le générateur
            System.Diagnostics.Debug.Write("--ConverterIsActive=" + __instance._dataResourceConverter.ConverterIsActive);
            // InitializeTemperature(__instance);
        }

        /** Heatshield **/

        /**
         * Permet de multiplier par 8 l'effet ablatif de la friction de l'air sur le bouclier thermique.
         * **/
        [HarmonyPatch(typeof(PartComponentModule_Heatshield), nameof(PartComponentModule_Heatshield.ResourceConsumptionUpdate))]
        [HarmonyPostfix]
        public static void ResourceConsumptionUpdatePostFix(PartComponentModule_Heatshield __instance)
        {
            if (__instance._dataHeatshield.requiredResources.Count <= 0)
                return;
                __instance._dataHeatshield.AblatorTonnesPerSecond = __instance._dataHeatshield.FluxRemoved * __instance._dataHeatshield.PyrolysisLossFactor * 8 * (double)__instance._dataHeatshield.requiredResources[0].Rate;
                __instance._requestConfigs[0].FlowUnits = __instance._dataHeatshield.AblatorTonnesPerSecond;
                __instance._dataHeatshield.AblatorRatio = (float)(__instance._containerGroup.GetResourceStoredUnits(__instance._inputResourcesIDs[0]) / __instance._containerGroup.GetResourceCapacityUnits(__instance._inputResourcesIDs[0]));
            __instance.resourceFlowRequestBroker.SetCommands(__instance._requestHandle, __instance._dataHeatshield.requiredResources[0].AcceptanceThreshold, __instance._requestConfigs.ToArray());
            System.Diagnostics.Debug.Write("Meltdown: AblatorTonnesPerSecond=" + __instance._dataHeatshield.AblatorTonnesPerSecond);
            System.Diagnostics.Debug.Write("Meltdown: AblatorRatio=" + __instance._dataHeatshield.AblatorRatio);
        }
    }
}

using HarmonyLib;
using I2.Loc;
using KSP;
using KSP.Modules;
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
            __instance.dataGenerator.AutoShutdown = true; // utile ?
            __instance.dataGenerator.FluxGenerated = 30000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
        }










        /** Resource_converter **/

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_ResourceConverter __instance) // tourne
        {
            __instance._dataResourceConverter.FluxGenerated = 300; // la valeur qui permet d'activer l'augmentation de la température
        }

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnToggleChangedValue))]
        [HarmonyPostfix]
        public static void OnToggleChangedValuePostFix(Module_ResourceConverter __instance) // fonctionne
        {
            __instance._dataResourceConverter.ConverterIsActive = !__instance._dataResourceConverter.ConverterIsActive; // nécessaire pour faire augmenter la température quand on active le générateur
        }

        /**
         * Fixes :
         * - the flux should be multiplied by the conversion rate
         * - the flux should not be multiplied by the time
         **/
        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.ThermalUpdate))]
        [HarmonyPostfix]
        public static void ThermalUpdatePostFix(double deltaTime, Module_ResourceConverter __instance)
        {           
            if (__instance._dataResourceConverter.ConverterIsActive && __instance._dataResourceConverter.FluxGenerated > 0.0)
            {
                __instance.part.Model.ThermalData.OtherFlux = __instance._dataResourceConverter.FluxGenerated * (double)__instance._dataResourceConverter.conversionRate.GetValue();
            }
            
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
            if (__instance._dataResourceConverter.ConverterIsActive != __state)
            {
                __instance._dataResourceConverter.ConverterIsActive = __state;
            }
        }











        /** Cooler **/

        /**
         * Fix the units not displaying when the cooler is removing heat.
         **/
        [HarmonyPatch(typeof(Module_Cooler), nameof(Module_Cooler.SetStatusString))]
        [HarmonyPostfix]
        public static void SetStatusStringPostFix(Module_Cooler __instance)
        {
            if (__instance.dataCooler.currentCoolerState == CoolerStates.REMOVING) {
                string statusDescription = LocalizationManager.GetTranslation(__instance.dataCooler.currentCoolerState.Description(), true, 0, true, false, (GameObject)null, (string)null, true);
                string energyRemoved = Units.PrintSI(__instance.dataCooler.fluxRemoved * 1000.0, "W", 3);
                __instance.dataCooler.coolerStatusText.SetValue(statusDescription + " " + energyRemoved);
            }
        }











        /** Active Radiator **/

        /**
         * ...
         **/
        [HarmonyPatch(typeof(PartComponentModule_ActiveRadiator), nameof(PartComponentModule_ActiveRadiator.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double universalTime, double deltaUniversalTime, PartComponentModule_ActiveRadiator __instance)
        {

        }

        /**
         * Define the currentCoolerState on initializing the radiator. It's OFF in the OAB, and RETRACTED by default when in flight.
         **/
        [HarmonyPatch(typeof(Module_ActiveRadiator), nameof(Module_ActiveRadiator.OnInitialize))]
        [HarmonyPostfix]
        public static void OnInitializePostFix(Module_ActiveRadiator __instance)
        {
            __instance.OnBeforeSetStatusString();
            //System.Diagnostics.Debug.Write("OnInitializePostFix: currentCoolerState=" + __instance.dataCooler.currentCoolerState); // works
        }






        /** Thermal Component **/

        /**
         * Add all radiator parts in the list of coolers. They will be taken into account later on (in OnUpdate) for the dissipation.
         **/
        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnStart))]
        [HarmonyPostfix]
        public static void OnStartPostFix(double universalTime, ThermalComponent __instance)
        {
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                PartComponentModule_ActiveRadiator module;
                if (part.TryGetModule<PartComponentModule_ActiveRadiator>(out module))
                {
                    __instance._coolingModules.AddUnique<PartComponentModule_Cooler>(module);
                }

            }
        }

        /**
         * Return true if the part is generating heat.
         **/
        private static bool isGeneretingHeat(PartComponent part)
        {
            PartComponentModule_ResourceConverter module;
            bool flagResourceConverter = part.TryGetModule<PartComponentModule_ResourceConverter>(out module) && module._dataResourceConverter.ConverterIsActive && module._dataResourceConverter.conversionRate.GetValue() != 0;
            return flagResourceConverter;
        }

        /**
         * The heat removing part is supposed to be done by ThermalComponentJob.FinalizeJob but for some reason this code doesn't seem to do what it is supposed to.
         * I was unable to get it to work as it's not patchable, so I'm doing this hack instead: altering the OtherFlux to take into account the heat removed by the radiators.
         * This flux should normally only contain the heat generated by the part.
         * 
         * Removes from each part of the ship the energy diffused by each radiator.
         * x100 to counteract PartComponentModule_Cooler.EnergyApplied that cannot be patched.
         * 
         * Prefix because it needs to run before the thermal jobs.
         **/
        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnUpdate))]
        [HarmonyPrefix]
        public static void OnUpdatePreFix(double universalTime, double deltaUniversalTime, ThermalComponent __instance)
        {
            int count = __instance._coolingModules.Count;
            if (count == 0) return;
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                if (!isGeneretingHeat(part)) continue; // if the current part isn't generating heat, there's not heat to dissipate.
                while (count-- > 0)
                {
                    if (!__instance._coolingModules[count].CoolerOperational) continue; // if the radiator is retracted, move on to the next one
                    part.ThermalData.OtherFlux -= __instance._coolingModules[count].EnergyApplied * 100;
                }
            }

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
        }
    }
}

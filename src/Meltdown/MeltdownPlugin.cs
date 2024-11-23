using HarmonyLib;
using KSP;
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
            __instance.dataGenerator.AutoShutdown = true; // utile ?
            __instance.dataGenerator.FluxGenerated = 30000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
        }

        /** Resource_converter **/

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_ResourceConverter __instance) // tourne
        {
            __instance._dataResourceConverter.FluxGenerated = 30000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
        }

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnToggleChangedValue))]
        [HarmonyPostfix]
        public static void OnToggleChangedValuePostFix(Module_ResourceConverter __instance) // fonctionne
        {
            __instance._dataResourceConverter.ConverterIsActive = !__instance._dataResourceConverter.ConverterIsActive; // nécessaire pour faire augmenter la température quand on active le générateur
        }

        /**
         * Fixes the generated flux not depending on the conversion rate.
         * **/
        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.ThermalUpdate))]
        [HarmonyPostfix]
        public static void ThermalUpdatePostFix(double deltaTime, Module_ResourceConverter __instance)
        {
            //ThermalData thermalData = __instance.part.Model.ThermalData;
            if (__instance._dataResourceConverter.ConverterIsActive && __instance._dataResourceConverter.FluxGenerated > 0.0)
            {
                __instance.part.Model.ThermalData.OtherFlux = __instance._dataResourceConverter.FluxGenerated * deltaTime * (double)__instance._dataResourceConverter.conversionRate.GetValue();
            }
            //__instance.part.Model.ThermalData = thermalData;
        }

        /** Cooler **/

        /**
         * Crude but effective fix to the units not displaying.
         * **/
        [HarmonyPatch(typeof(Module_Cooler), nameof(Module_Cooler.SetStatusString))]
        [HarmonyPostfix]
        public static void SetStatusStringPostFix(Module_Cooler __instance)
        {
            if (__instance.dataCooler.currentCoolerState == CoolerStates.REMOVING) {
                __instance.dataCooler.coolerStatusText.SetValue(__instance.dataCooler.coolerStatusText.GetValue() + " kW");
            }
        }

        /**
         * Fix the cooler removing heat when it's retracted.
         * [Warning:  HarmonyX] AccessTools.DeclaredMethod: Could not find method for type KSP.Sim.impl.PartComponentModule_Cooler and name CoolerOperational and parameters 
[Error  :  Meltdown] HarmonyLib.HarmonyException: Patching exception in method null ---> System.ArgumentException: Undefined target method for patch method static void Meltdown.MeltdownPlugin::CoolerOperationalPostFix(KSP.Sim.impl.PartComponentModule_Cooler __instance, Boolean& __result)
         * **/
        //[HarmonyPatch(typeof(PartComponentModule_Cooler), nameof(PartComponentModule_Cooler.CoolerOperational))]
        //[HarmonyPostfix]
        //public static void CoolerOperationalPostFix(ref bool __result)
        //{
        //    System.Diagnostics.Debug.Write("CoolerOperationalPostFix: __result=" + __result);
        //}

        /** Active Radiator **/

        /**
         * Fix the radiator removing heat when it's retracted.
         * **/
        [HarmonyPatch(typeof(PartComponentModule_ActiveRadiator), nameof(PartComponentModule_ActiveRadiator.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double universalTime, double deltaUniversalTime, PartComponentModule_ActiveRadiator __instance)
        {
            System.Diagnostics.Debug.Write("OnUpdatePostFix: currentCoolerState=" + __instance.dataCooler.currentCoolerState);
            if (__instance.dataCooler.currentCoolerState == CoolerStates.RETRACTED) { }// to be tested x2

            //__instance.dataCooler.fluxRemoved = 0.0;
            else
            {
                //__instance.dataCooler.fluxRemoved = 200.0; // provisoire, ne pas le fixer
            }
             //System.Diagnostics.Debug.Write("OnUpdatePostFix: fluxRemoved=" + __instance.dataCooler.fluxRemoved);
         }

        /**
         * Define the currentCoolerState on initializing the radiator. It's OFF in the OAB, and RETRACTED by default when in flight.
         * **/
        [HarmonyPatch(typeof(Module_ActiveRadiator), nameof(Module_ActiveRadiator.OnInitialize))]
        [HarmonyPostfix]
        public static void OnInitializePostFix(Module_ActiveRadiator __instance)
        {
            __instance.OnBeforeSetStatusString();
            //System.Diagnostics.Debug.Write("OnInitializePostFix: currentCoolerState=" + __instance.dataCooler.currentCoolerState); // works
        }


        /** Thermal Component **/

        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double universalTime, double deltaUniversalTime, ThermalComponent __instance)
        {
            int count = __instance._coolingModules.Count;
            System.Diagnostics.Debug.Write("ThermalComponent.OnUpdatePostFix: count=" + count); // 0
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                ThermalData thermalData = part.ThermalData;
                while (count-- > 0)
                {
                    __instance._coolingModules[count].ThermalCooling(__instance._deltaUniverseTime);
                    System.Diagnostics.Debug.Write("ThermalComponent.OnUpdatePostFix: [" + count + "]/CoolerOperational=" + __instance._coolingModules[count].CoolerOperational);
                    System.Diagnostics.Debug.Write("ThermalComponent.OnUpdatePostFix: [" + count + "]/EnergyApplied=" + __instance._coolingModules[count].EnergyApplied);
                }
                System.Diagnostics.Debug.Write("ThermalComponent.OnUpdatePostFix: " + part.Name + "/CoolingEnergyToApply=" + thermalData.CoolingEnergyToApply); // retourne 0 !
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
            //System.Diagnostics.Debug.Write("Meltdown: AblatorTonnesPerSecond=" + __instance._dataHeatshield.AblatorTonnesPerSecond);
            //System.Diagnostics.Debug.Write("Meltdown: AblatorRatio=" + __instance._dataHeatshield.AblatorRatio);
        }
    }
}

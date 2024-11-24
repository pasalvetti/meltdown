﻿using HarmonyLib;
using I2.Loc;
using KSP;
using KSP.Iteration.UI.Binding;
using KSP.Modules;
using KSP.Sim.impl;
using Meltdown.Modules;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;

namespace Meltdown
{
    internal class MeltdownPlugin
    {

        /** Module_Generator **/

        /**
         * Enable heat generation for generators.
         * Prefix so that the field gets displayed in SetPAMVisibility().
         **/
        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.OnInitialize))]
        [HarmonyPrefix]
        static public void OnInitializePreFix(Module_Generator __instance) // tourne
        {
            __instance.dataGenerator.AutoShutdown = true; // utile ?
            __instance.dataGenerator.FluxGenerated = 300;
            //if (__instance.part != null && __instance.part.Model != null && !__instance.part.Model.ThermalData.Equals(null))
            //{
            //    __instance.part.Model.ThermalData.ThermalMass = 25;
            //    System.Diagnostics.Debug.Write("Module_Generator.OnInitialize: thermalMass=" + __instance.part.Model.ThermalData.ThermalMass); // ok
            //}

        }

        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.ThermalUpdate))]
        [HarmonyPostfix]
        public static void ThermalUpdatePostFix(double deltaTime, Module_Generator __instance)
        {
            //if (__instance._engineStatus!=null)
            //{
            //    System.Diagnostics.Debug.Write("Module_Generator.ThermalUpdatePostFix: normalizedOutput=" + __instance._engineStatus.normalizedOutput);
            //} else
            //{
            //    System.Diagnostics.Debug.Write("Module_Generator.ThermalUpdatePostFix: _engineStatus est null");
            //}
            //System.Diagnostics.Debug.Write("Module_Generator.ThermalUpdatePostFix: otherFlux=" + __instance.part.Model.ThermalData.OtherFlux);
            //__instance.part.Model.ThermalData.OtherFlux *= 10; // for balance
            //System.Diagnostics.Debug.Write("Module_Generator.ThermalUpdatePostFix: thermalMass=" + __instance.part.Model.ThermalData.ThermalMass); // 1000
        }

        //[HarmonyPatch(typeof(PartComponentModule_Generator), nameof(PartComponentModule_Generator.OnStart))]
        //[HarmonyPostfix]
        //public static void OnStartPostFix(double universalTime, PartComponentModule_Generator __instance)
        //{
        //    PartComponentModule_Thermal thermalComponent;
        //    if (__instance.Part.TryGetModule<PartComponentModule_Thermal>(out thermalComponent))
        //    {
        //        System.Diagnostics.Debug.Write("PartComponentModule_Generator.OnStartPostFix: thermal found.");
        //        if (thermalComponent._dataThermal != null) // no
        //        {
        //            System.Diagnostics.Debug.Write("PartComponentModule_Generator.OnStartPostFix: thermalMass=" + thermalComponent._dataThermal.thermalMass);
        //            __instance.Part.ThermalData.ThermalMass = thermalComponent._dataThermal.thermalMass;
        //        }

        //    }
        // }
        
    




        /** Resource_converter **/

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_ResourceConverter __instance)
        {
            __instance._dataResourceConverter.FluxGenerated = 300; // this is what enables heat generation for resource converters
            //System.Diagnostics.Debug.Write("Module_ResourceConverter.OnInitialize: thermalMass=" + __instance.part.Model.ThermalData.ThermalMass);
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
            //System.Diagnostics.Debug.Write("Module_ResourceConverter.ThermalUpdatePostFix: otherFlux=" + __instance.part.Model.ThermalData.OtherFlux);
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
            bool flagResourceConverter = part.TryGetModule<PartComponentModule_ResourceConverter>(out PartComponentModule_ResourceConverter module) && module._dataResourceConverter.ConverterIsActive && module._dataResourceConverter.conversionRate.GetValue() != 0;
            bool flagGenerator = part.TryGetModule<PartComponentModule_Generator>(out _);
            //if (!flagResourceConverter && module != null)
            //{
            //    System.Diagnostics.Debug.Write("isGeneretingHeat: " + part.PartName + " " + part.GlobalId + " ConverterIsActive=" + module._dataResourceConverter.ConverterIsActive);
            //    System.Diagnostics.Debug.Write("isGeneretingHeat: " + part.PartName + " " + part.GlobalId + " conversionRate=" + module._dataResourceConverter.conversionRate.GetValue());
            //}
            return flagResourceConverter || flagGenerator;
        }

        private static int getNumberOfHeatingParts(ThermalComponent __instance)
        {
            int numberOfHeatingParts = 0;
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                if (isGeneretingHeat(part)) numberOfHeatingParts++;
            }
            return numberOfHeatingParts;
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
        public static void OnUpdatePreFix(double universalTime, double deltaUniversalTime, ThermalComponent __instance, ref int __state)
        {
            int numnberOfRadiators = __instance._coolingModules.Count;
            int numberOfHeatingParts = getNumberOfHeatingParts(__instance);
            __state = numberOfHeatingParts;
            //System.Diagnostics.Debug.Write("OnUpdatePreFix: numberOfHeatingParts=" + numberOfHeatingParts);
            if (numberOfHeatingParts * numnberOfRadiators == 0) return; // if no part is generating heat, or if there's no radiator, there's no heat to dissipate
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                int i = numnberOfRadiators;
                //System.Diagnostics.Debug.Write("OnUpdatePreFix: " + part.PartName + " " + part.GlobalId + " otherFlux before fix: " + part.ThermalData.OtherFlux);
                if (!isGeneretingHeat(part)) continue; // if the current part isn't generating heat, there's not heat to dissipate.
                while (i-- > 0)
                {
                    if (!__instance._coolingModules[i].CoolerOperational) continue; // if the radiator is retracted, move on to the next one
                    part.ThermalData.OtherFlux -= (__instance._coolingModules[i].EnergyApplied * 100 / numberOfHeatingParts);
                    
                    //System.Diagnostics.Debug.Write("OnUpdatePreFix: " + part.PartName + " " + part.GlobalId + " otherFlux fixed: " + part.ThermalData.OtherFlux);
                }
            }

        }

        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double universalTime, double deltaUniversalTime, ThermalComponent __instance, ref int __state)
        {
            int numberOfHeatingParts = __state;
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                //part.ThermalData.CoolingDemand = 0; // this is blocking cooling in FinalizeJob.Execute since it's only working for generators
                if (numberOfHeatingParts != 0) //ne fonctionne pas bien car les générateurs ne sont pas considérés comme heating part
                {
                    part.ThermalData.CoolingEnergyToApply = part.ThermalData.CoolingEnergyToApply / numberOfHeatingParts; // mainly for the display in the debug window, but has an effect on FinalizeJob.Execute as well
                }
                else
                {
                    part.ThermalData.CoolingEnergyToApply = 0.0;
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

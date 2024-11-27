using HarmonyLib;
using I2.Loc;
using KSP;
using KSP.Iteration.UI.Binding;
using KSP.Logging;
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

        /**
         * Sets the 'isHeating' flag, used to mark the part as heating or not when calculating the radiators' influence.
         * Also sets the flux (in kW). If no flux is to be set, use usePatchedFlux=false (only to be used if the stock game already generates the flux).
         **/
        private static void SetThermalFluxData(PartComponentModule __instance, bool isHeating, bool usePatchedFlux)
        {
            if (__instance.Part.TryGetModule<PartComponentModule_Thermal>(out PartComponentModule_Thermal thermalComponent))
            {
                if (thermalComponent._dataThermal == null) return;
                thermalComponent._dataThermal.isHeating = isHeating;
                if (usePatchedFlux)
                {
                    thermalComponent.SetFlux();
                }
            }
        }












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
        }

        /**
         * Marks the generator as a heat-generating part.
         **/
        [HarmonyPatch(typeof(PartComponentModule_Generator), nameof(PartComponentModule_Generator.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double universalTime, PartComponentModule_Generator __instance)
        {
            SetThermalFluxData(__instance, true, false); // a generator is always heating. The flux is 0.0 because it's already set in the stock module.
        }
        
    




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
            /* Marks as heating if the converter is on and has a rate > 0. The flux is 0.0 because it's already set in the stock module. */
            SetThermalFluxData(__instance._componentModule, __instance._dataResourceConverter.ConverterIsActive && __instance._dataResourceConverter.conversionRate.GetValue() > 0, false);

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
        private static void CacheAllRadiatorsAsCoolingModules(ThermalComponent __instance)
        {
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                if (part.TryGetModule<PartComponentModule_ActiveRadiator>(out PartComponentModule_ActiveRadiator module))
                {
                    __instance._coolingModules.AddUnique<PartComponentModule_Cooler>(module);
                }
            }
        }

        /**
         * On component start, all cooling modules gets cached, but not radiators. We need to add them too.
         **/
        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnStart))]
        [HarmonyPostfix]
        public static void OnStartPostFix(ThermalComponent __instance)
        {
            CacheAllRadiatorsAsCoolingModules(__instance);
        }

        /**
         * After a part is destroyed, RecacheCoolingModules gets called and clears our list of radiators, so we need to recache them.
         **/
        [HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.RecacheCoolingModules))]
        [HarmonyPostfix]
        private static void RecacheCoolingModulesPostFix(ThermalComponent __instance)
        {
            CacheAllRadiatorsAsCoolingModules(__instance);
        }

        /**
         * Return true if the part is generating heat.
         **/
        private static bool IsGeneretingHeat(PartComponent part)
        {
            if (part.TryGetModule<PartComponentModule_Thermal>(out PartComponentModule_Thermal thermalModule))
            {
                if (thermalModule._dataThermal == null) return false;
                //System.Diagnostics.Debug.Write("isGeneretingHeat: " + part.PartName + " " + part.GlobalId + " yes !");
                return thermalModule._dataThermal.isHeating;
            }
            return false;
        }

        private static double GetTotalThermalEnergy(ThermalComponent __instance)
        {
            //numberOfHeatingParts = 0;
            double totalThermalEnergy = 0.0;
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                if (part.ThermalData.Equals(null)) continue;
                if (IsGeneretingHeat(part))
                {
                    //numberOfHeatingParts++;
                    totalThermalEnergy += (getTotalThermalEnergyOfPart(part));
                }
            }
            return totalThermalEnergy;
        }

        private static double getTotalThermalEnergyOfPart(PartComponent part)
        {
            return part.ThermalData.OtherFlux + part.ThermalData.EnvironmentFlux + part.ThermalData.ExhaustFlux + part.ThermalData.ReentryFlux + part.ThermalData.SolarFlux;
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
        public static void OnUpdatePreFix(ThermalComponent __instance, ref double __state)
        {
            int numberOfRadiators = __instance._coolingModules.Count;
            //System.Diagnostics.Debug.Write("OnUpdatePreFix: numberOfRadiators=" + numberOfRadiators);
            double totalThermalEnergy = GetTotalThermalEnergy(__instance);
            //System.Diagnostics.Debug.Write("OnUpdatePreFix: totalThermalEnergy=" + totalThermalEnergy);
            __state = totalThermalEnergy;
            //System.Diagnostics.Debug.Write("OnUpdatePreFix: numberOfHeatingParts=" + numberOfHeatingParts);
            if (totalThermalEnergy == 0.0) return; // if no part is generating heat, there's no heat to dissipate
            foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
            {
                int i = numberOfRadiators;
                //System.Diagnostics.Debug.Write("OnUpdatePreFix: " + part.PartName + " " + part.GlobalId + " getTotalThermalEnergyOfPart=" + getTotalThermalEnergyOfPart(part));
                double energyRemoved = 0.0;
                if (IsGeneretingHeat(part))  // if the current part is generating heat, there's heat to dissipate.
                {
                    while (i-- > 0)
                    {
                        if (!__instance._coolingModules[i].CoolerOperational) continue; // if the radiator is retracted, move on to the next one
                        energyRemoved += (__instance._coolingModules[i].EnergyApplied * 100 * getTotalThermalEnergyOfPart(part) / totalThermalEnergy);

                        //System.Diagnostics.Debug.Write("OnUpdatePreFix: " + part.PartName + " " + part.GlobalId + " otherFlux fixed: " + part.ThermalData.OtherFlux);
                    }
                }
                if (part.TryGetModule<PartComponentModule_Thermal>(out PartComponentModule_Thermal thermalModule) && thermalModule._dataThermal != null)
                {
                    thermalModule._dataThermal.energyRemoved = energyRemoved; // we store the removed energy for display on the debug window
                }
                part.ThermalData.OtherFlux -= energyRemoved; // we substract the removed energy in the other flux (dirty hack)
            }
        }

        //[HarmonyPatch(typeof(ThermalComponent), nameof(ThermalComponent.OnUpdate))]
        //[HarmonyPostfix]
        //public static void OnUpdatePostFix(double universalTime, double deltaUniversalTime, ThermalComponent __instance, ref double __state)
        //{
        //    double totalThermalEnergy = __state;
        //    foreach (PartComponent part in __instance.SimulationObject.PartOwner.Parts)
        //    {
        //        if (totalThermalEnergy != 0 && IsGeneretingHeat(part))
        //        {
        //            System.Diagnostics.Debug.Write("OnUpdatePostFix: " + part.PartName + " " + part.GlobalId + " CoolingEnergyToApply (before)=" + part.ThermalData.CoolingEnergyToApply);
        //            part.ThermalData.CoolingEnergyToApply = part.ThermalData.CoolingEnergyToApply * getTotalThermalEnergyOfPart(part) / totalThermalEnergy; // mainly for the display in the debug window, but has an effect on FinalizeJob.Execute as well
        //            System.Diagnostics.Debug.Write("OnUpdatePostFix: " + part.PartName + " " + part.GlobalId + " CoolingEnergyToApply (after)=" + part.ThermalData.CoolingEnergyToApply);
        //        } else
        //        {
        //            part.ThermalData.CoolingEnergyToApply = 0.0;
        //        }
        //    }
        //}













        /** Command **/

        [HarmonyPatch(typeof(PartComponentModule_Command), nameof(PartComponentModule_Command.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(PartComponentModule_Command __instance)
        {
            SetThermalFluxData(__instance, true, true); // a command pod is always heating
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

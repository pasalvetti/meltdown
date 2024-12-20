﻿using HarmonyLib;
using KSP.Modules;
using KSP.Sim.impl;

namespace Meltdown.Patches
{
    internal class SolarPanelPatches
    {
        [HarmonyPatch(typeof(PartComponentModule_SolarPanel), nameof(PartComponentModule_SolarPanel.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(double deltaUniversalTime, PartComponentModule_SolarPanel __instance)
        {
            ////System.Diagnostics.Debug.Write("[Meltdown] PartComponentModule_SolarPanel.OnUpdatePostFix");
            //System.Diagnostics.Debug.Write("[Meltdown] PartComponentModule_SolarPanel.OnUpdatePostFix: deltaUniversalTime=" + deltaUniversalTime);
            //if (deltaUniversalTime > 1)
            //{
            //    //MeltdownPlugin.GenerateFlux(__instance, isHeating: false, rate:0.0, usePatchedFlux: true);
            //} else
            //{
            //    Data_Deployable.DeployState deployState = __instance.dataDeployable.CurrentDeployState.GetValue();
            //    bool IsActive = (!__instance.dataDeployable.extendable || deployState == Data_Deployable.DeployState.Extended || deployState == Data_Deployable.DeployState.Extending);
            //    double rate = __instance.dataSolarPanel.EnergyFlow.GetValue() / __instance.dataSolarPanel.ResourceSettings.Rate;
            //    System.Diagnostics.Debug.Write("[Meltdown] PartComponentModule_SolarPanel.OnUpdatePostFix: " + __instance.Part.Name + " " + __instance.Part.GlobalId + "/rate=" + rate);
            //    MeltdownPlugin.GenerateFlux(__instance, isHeating: IsActive, rate, usePatchedFlux: true);
            //}
        }
    }
}

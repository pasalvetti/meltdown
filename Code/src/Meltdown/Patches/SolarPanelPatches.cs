using HarmonyLib;
using KSP.Modules;
using KSP.Sim.impl;

namespace Meltdown.Patches
{
    internal class SolarPanelPatches
    {
        [HarmonyPatch(typeof(PartComponentModule_SolarPanel), nameof(PartComponentModule_SolarPanel.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(PartComponentModule_SolarPanel __instance)
        {
            Data_Deployable.DeployState deployState = __instance.dataDeployable.CurrentDeployState.GetValue();
            bool IsActive = (!__instance.dataDeployable.extendable || deployState == Data_Deployable.DeployState.Extended || deployState == Data_Deployable.DeployState.Extending);
            double rate = __instance.dataSolarPanel.EnergyFlow.GetValue() / __instance.dataSolarPanel.ResourceSettings.Rate;
            MeltdownPlugin.GenerateFlux(__instance, isHeating:IsActive, rate, usePatchedFlux:true);
        }
    }
}

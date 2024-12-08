using HarmonyLib;
using I2.Loc;
using KSP.Modules;
using KSP.Sim.impl;
using KSP;
using UnityEngine;

namespace Meltdown.Patches
{
    internal class CoolerPatches
    {
        /**
         * Fix the units not displaying when the cooler is removing heat.
         **/
        [HarmonyPatch(typeof(Module_Cooler), nameof(Module_Cooler.SetStatusString))]
        [HarmonyPostfix]
        public static void SetStatusStringPostFix(Module_Cooler __instance)
        {
            if (__instance.dataCooler.currentCoolerState == CoolerStates.REMOVING)
            {
                string statusDescription = LocalizationManager.GetTranslation(__instance.dataCooler.currentCoolerState.Description(), true, 0, true, false, (GameObject)null, (string)null, true);
                string energyRemoved = Units.PrintSI(__instance.dataCooler.fluxRemoved * 1000.0, "W", 3);
                __instance.dataCooler.coolerStatusText.SetValue(statusDescription + " " + energyRemoved);
            }
        }
    }
}

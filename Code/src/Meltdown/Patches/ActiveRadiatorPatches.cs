using HarmonyLib;
using KSP.Modules;

namespace Meltdown.Patches
{
    internal class ActiveRadiatorPatches
    {
        /**
         * Defines the currentCoolerState on initializing the radiator. It's OFF in the OAB, and RETRACTED by default when in flight.
         **/
        [HarmonyPatch(typeof(Module_ActiveRadiator), nameof(Module_ActiveRadiator.OnInitialize))]
        [HarmonyPostfix]
        public static void OnInitializePostFix(Module_ActiveRadiator __instance)
        {
            __instance.OnBeforeSetStatusString();
        }
    }
}

using HarmonyLib;
using KSP.Sim.impl;

namespace Meltdown.Patches
{
    internal class CommandPatches
    {
        [HarmonyPatch(typeof(PartComponentModule_Command), nameof(PartComponentModule_Command.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostFix(PartComponentModule_Command __instance)
        {
            bool isHeating = __instance._hasResourcesToOperate; // a command pod is heating unless it's out of EC
            MeltdownPlugin.GenerateFlux(__instance, isHeating, rate:1.0, usePatchedFlux:true);
        }
    }
}

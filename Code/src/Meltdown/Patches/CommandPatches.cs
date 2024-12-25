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
            bool isHeating = __instance._hasResourcesToOperate && !__instance.dataCommand.IsHibernating; // a command pod is heating when it's neither hibernating nor out of EC
            MeltdownPlugin.GenerateFlux(__instance, isHeating, rate:1.0, usePatchedFlux:true);
        }
    }
}

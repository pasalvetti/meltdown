using HarmonyLib;
using KSP.Api;
using KSP.Modules;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using UnityEngine;

namespace Meltdown
{
    internal class MeltdownPlugin
    {
        static double temperatureMod = -1;
        //static double otherFluxMod = 0;

        [LocalizedField("PartModules/ResourceConverter/OutputMod")]
        [PAMDisplayControl(SortIndex = 8)]
        [HideInInspector]
        static ModuleProperty<string> OutputTxtMod = new ModuleProperty<string>("lalaland", true, new ToStringDelegate(Data_ResourceConverter.GetConversionOutputString));

        /** Module_Generator **/

        [HarmonyPatch(typeof(Module_Generator), nameof(Module_Generator.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_Generator __instance) // tourne
        {
            //System.Diagnostics.Debug.Write("Meltdown: OnInitializePostFix");
            __instance.dataGenerator.AutoShutdown = true; // utile ?
            __instance.dataGenerator.FluxGenerated = 10000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
            //System.Diagnostics.Debug.Write("--FluxGenerated=" + __instance.dataGenerator.FluxGenerated); // ok
        }

        /** Resource_converter **/

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnInitialize))]
        [HarmonyPostfix]
        static public void OnInitializePostFix(Module_ResourceConverter __instance) // tourne
        {
            //System.Diagnostics.Debug.Write("Meltdown: Module_ResourceConverter.OnInitializePostFix");
            __instance._dataResourceConverter.FluxGenerated = 10000; // la valeur qui permet d'activer l'augmentation de la température (100 000 = explose en 1 s)
            //System.Diagnostics.Debug.Write("--FluxGenerated=" + __instance._dataResourceConverter.FluxGenerated); // ok
        }

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.OnToggleChangedValue))]
        [HarmonyPostfix]
        public static void OnToggleChangedValuePostFix(Module_ResourceConverter __instance) // fonctionne
        {
            System.Diagnostics.Debug.Write("Meltdown: OnToggleChangedValuePostFix");
            __instance._dataResourceConverter.ConverterIsActive = !__instance._dataResourceConverter.ConverterIsActive; // nécessaire pour faire augmenter la température quand on active le générateur
            System.Diagnostics.Debug.Write("--ConverterIsActive=" + __instance._dataResourceConverter.ConverterIsActive);
            // InitializeTemperature(__instance);
        }

        [HarmonyPatch(typeof(Module_ResourceConverter), nameof(Module_ResourceConverter.UpdatePAMVisibility))]
        [HarmonyPrefix]
        public static void UpdatePAMVisibilityPostfix(bool state, Module_ResourceConverter __instance)
        {
            System.Diagnostics.Debug.Write("Meltdown: Module_ResourceConverter.UpdatePAMVisibility");
            __instance._dataResourceConverter.SetVisible((IModuleDataContext)OutputTxtMod, true); // [Error  :UnityExplorer] [Unity] [Debug] Cannot change visibility on a NULL PropertyContextKey
            //System.Diagnostics.Debug.Write("--Temperature=" + __instance.part.Model.ThermalData.Temperature);
        }

        [HarmonyPatch(typeof(PartComponentModule_ResourceConverter), nameof(PartComponentModule_ResourceConverter.OnUpdate))]
        [HarmonyPostfix]
        public static void OnUpdatePostfix(double universalTime, double deltaUniversalTime, PartComponentModule_ResourceConverter __instance)
        {
            //System.Diagnostics.Debug.Write("Meltdown: PartComponentModule_ResourceConverter.OnUpdate");
            if (__instance._dataResourceConverter != null && __instance._dataResourceConverter.OutputTxt != null)
            {
                //__instance._dataResourceConverter.OutputTxt.SetValue("polo");
                __instance._dataResourceConverter.OutputTxt.SetValue(Convert.ToInt32(__instance.Part.Temperature).ToString());
                OutputTxtMod.SetValue(__instance.Part.Temperature.ToString());
                //System.Diagnostics.Debug.Write("--OutputTxt=" + __instance._dataResourceConverter.OutputTxt.GetValue());
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
            System.Diagnostics.Debug.Write("Meltdown: AblatorTonnesPerSecond=" + __instance._dataHeatshield.AblatorTonnesPerSecond);
            System.Diagnostics.Debug.Write("Meltdown: AblatorRatio=" + __instance._dataHeatshield.AblatorRatio);
        }
    }
}

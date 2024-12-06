using I2.Loc;
using KSP;
using KSP.Api;
using KSP.Sim;
using KSP.Sim.Definitions;
using Newtonsoft.Json;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Data_Thermal : ModuleData
    {
        public override Type ModuleType => typeof(Module_Thermal);

        [LocalizedField("PartModules/Thermal/HeatGenerated")]
        [PAMDisplayControl(SortIndex = 1)]
        [JsonIgnore]
        public ModuleProperty<double> HeatGeneratedTxt = new ModuleProperty<double>(0.0, true, new ToStringDelegate(Data_Thermal.GetHeatOutputString));

        [LocalizedField("PartModules/Thermal/Temperature")]
        [PAMDisplayControl(SortIndex = 2)]
        [Tooltip("Part temperature")]
        public ModuleProperty<string> TemperatureTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/EnvironmentFlux")]
        [PAMDisplayControl(SortIndex = 3)]
        public ModuleProperty<string> EnvironmentFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/SolarFlux")]
        [PAMDisplayControl(SortIndex = 4)]
        public ModuleProperty<string> SolarFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/ReentryFlux")]
        [PAMDisplayControl(SortIndex = 5)]
        public ModuleProperty<string> ReentryFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/ExhaustFlux")]
        [PAMDisplayControl(SortIndex = 6)]
        public ModuleProperty<string> ExhaustFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/OtherFlux")]
        [PAMDisplayControl(SortIndex = 7)]
        public ModuleProperty<string> otherFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/CoolingEnergyToApply")]
        [PAMDisplayControl(SortIndex = 8)]
        public ModuleProperty<string> CoolingEnergyToApplyTxt = new("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [KSPDefinition]
        public double FluxGenerated;
        [KSPDefinition]
        public double ThermalMass = 0.0;

        private static string GetConversionOutputString(object valueObj) => (string)valueObj;

        private static string GetHeatOutputString(object valueObj) => string.Format("{0:F1} {1}", (object)Math.Abs((double)valueObj), (object)Units.SymbolKiloWatt);

        public bool isHeating = false;

        public double energyRemoved;

        public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType, List<OABPartData.PartInfoModuleEntry> delegateList)
        {
            if (partBehaviourModuleType == this.ModuleType && ThermalMass != 0.0)
                delegateList.Add(new OABPartData.PartInfoModuleEntry(LocalizationManager.GetTranslation("Menu/VAB/thermalMass", true, 0, true, false, (GameObject)null, (string)null, true), new OABPartData.PartInfoModuleMultipleEntryValueDelegate(this.GetResourceStrings)));
            return delegateList;
        }

        private List<OABPartData.PartInfoModuleSubEntry> GetResourceStrings(
          OABPartData.OABSituationStats oabSituationStats)
        {
            List<OABPartData.PartInfoModuleSubEntry> resourceStrings = new List<OABPartData.PartInfoModuleSubEntry>();
            //for (int index = 0; index < this.requiredResources.Count; ++index)
                resourceStrings.Add(new OABPartData.PartInfoModuleSubEntry(LocalizationManager.GetTranslation("VAB/Fuel/ElectricCharge", true, 0, true, false, (GameObject)null, (string)null, true), ThermalMass.ToString() + " J/K"));
            return resourceStrings;
        }

    }
}

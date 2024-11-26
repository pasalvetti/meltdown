using KSP;
using KSP.Api;
using KSP.Modules;
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
        public ModuleProperty<string> CoolingEnergyToApplyTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        private static string GetConversionOutputString(object valueObj) => (string)valueObj;

        private static string GetHeatOutputString(object valueObj) => string.Format("{0:F1} {1}", (object)Math.Abs((double)valueObj), (object)Units.SymbolKiloWatt);

        public bool isHeating = false;

    }
}

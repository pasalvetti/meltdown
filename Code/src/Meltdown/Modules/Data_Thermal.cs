using KSP.Api;
using KSP.Sim.Definitions;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Data_Thermal : ModuleData
    {
        public override Type ModuleType => typeof(Module_Thermal);

        [LocalizedField("PartModules/Thermal/Temperature")]
        [PAMDisplayControl(SortIndex = 1)]
        [Tooltip("Part temperature")]
        public ModuleProperty<string> TemperatureTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        [LocalizedField("PartModules/Thermal/EnvironmentFlux")]
        [PAMDisplayControl(SortIndex = 2)]
        public ModuleProperty<string> EnvironmentFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));


        [LocalizedField("PartModules/Thermal/SolarFlux")]
        [PAMDisplayControl(SortIndex = 3)]
        public ModuleProperty<string> SolarFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));


        [LocalizedField("PartModules/Thermal/ReentryFlux")]
        [PAMDisplayControl(SortIndex = 4)]
        public ModuleProperty<string> ReentryFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));


        [LocalizedField("PartModules/Thermal/ExhaustFlux")]
        [PAMDisplayControl(SortIndex = 5)]
        public ModuleProperty<string> ExhaustFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));


        [LocalizedField("PartModules/Thermal/OtherFlux")]
        [PAMDisplayControl(SortIndex = 6)]
        public ModuleProperty<string> otherFluxTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));


        [LocalizedField("PartModules/Thermal/CoolingEnergyToApply")]
        [PAMDisplayControl(SortIndex = 6)]
        public ModuleProperty<string> CoolingEnergyToApplyTxt = new ModuleProperty<string>("", true, new ToStringDelegate(Data_Thermal.GetConversionOutputString));

        private static string GetConversionOutputString(object valueObj) => (string)valueObj;

    }
}

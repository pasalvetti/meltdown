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

        private static string GetConversionOutputString(object valueObj) => (string)valueObj;

    }
}

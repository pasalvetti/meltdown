using KSP.Modules;
using KSP.Sim.Definitions;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Module_Thermal : PartBehaviourModule
    {
        [SerializeField]
        protected Data_Thermal _dataThermal;

        private readonly bool debugMode = true;
        private bool heatGeneratedVisibility = true;

        public override Type PartComponentModuleType => typeof(PartComponentModule_Thermal);

        public override void AddDataModules()
        {
            base.AddDataModules();
            this.DataModules.TryAddUnique<Data_Thermal>(_dataThermal, out _dataThermal);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            SetHeatGeneratedVisibility();
            UpdatePAMVisibility();
        }

        /**
         * No need to show the heat generated for Resource Converters and Generators as it's already done by the stock module.
         **/
        private void SetHeatGeneratedVisibility()
        {
            if (part == null) { return; }
            bool isGenerator = part.TryGetComponent<Module_Generator>(out _);
            bool isEngine = part.TryGetComponent<Module_Engine>(out _);
            heatGeneratedVisibility = !isGenerator || isEngine;
        }

        private void UpdatePAMVisibility()
        {
            if (_dataThermal == null) return;
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.HeatGeneratedTxt, heatGeneratedVisibility && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.TemperatureTxt, PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.EnvironmentFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.SolarFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ReentryFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ExhaustFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.otherFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.CoolingEnergyToApplyTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
        }

    }
}

﻿using KSP;
using KSP.Logging;
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
            if (part.TryGetComponent<Module_ResourceConverter>(out _) || part.TryGetComponent<Module_Generator>(out _))
            {
                heatGeneratedVisibility = false;
            }
        }

        private void UpdatePAMVisibility()
        {
            if (_dataThermal == null) return;
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.HeatGeneratedTxt, heatGeneratedVisibility && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.TemperatureTxt, PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.EnvironmentFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.SolarFluxTxt, false && debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ReentryFluxTxt, false && debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ExhaustFluxTxt, false && debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.otherFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.CoolingEnergyToApplyTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
        }

    }
}

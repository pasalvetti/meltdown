﻿using KSP.Modules;
using KSP.Sim.Definitions;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Module_Thermal : PartBehaviourModule
    {
        [SerializeField]
        protected Data_Thermal _dataThermal;

        private readonly bool debugMode = false;

        public override Type PartComponentModuleType => typeof(PartComponentModule_Thermal);

        public override void AddDataModules()
        {
            base.AddDataModules();
            this.DataModules.TryAddUnique<Data_Thermal>(_dataThermal, out _dataThermal);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
             UpdatePAMVisibility();
        }

        private void UpdatePAMVisibility()
        {
            if (_dataThermal == null) return;
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.HeatGeneratedTxt, PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.TemperatureTxt, PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.EnvironmentFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.SolarFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ReentryFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.ExhaustFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.otherFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            _dataThermal.SetVisible((IModuleDataContext)_dataThermal.CoolingEnergyToApplyTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
        }

        /**
         * Implements the ThermalUpdate for solar panels. This gets automatically called by the game.
         **/
        public override void ThermalUpdate(double deltaTime)
        {
            if (!part.TryGetComponent<Module_SolarPanel>(out Module_SolarPanel solarPanelModule)) return;
            //if (solarPanelModule._timeWarpActive) return;
            Data_Deployable.DeployState deployState = solarPanelModule.dataDeployable.CurrentDeployState.GetValue();
            bool IsActive = (!solarPanelModule.dataDeployable.extendable || deployState == Data_Deployable.DeployState.Extended || deployState == Data_Deployable.DeployState.Extending);
            double rate = solarPanelModule.dataSolarPanel.EnergyFlow.GetValue() / solarPanelModule.dataSolarPanel.ResourceSettings.Rate;
            //System.Diagnostics.Debug.Write("[Meltdown] Module_Thermal.OnUpdatePostFix: " + part.Model.Name + " " + part.Model.GlobalId + "/rate=" + rate);
            MeltdownPlugin.GenerateFlux(solarPanelModule._componentModule, isHeating: IsActive, rate, usePatchedFlux: true);
        }
    }
}

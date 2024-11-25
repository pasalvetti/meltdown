using KSP.Modules;
using KSP.Sim.Definitions;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Module_Thermal : PartBehaviourModule
    {
        [SerializeField]
        protected Data_Thermal _dataThermal;

        private bool debugMode = true;

        public override Type PartComponentModuleType => typeof(PartComponentModule_Thermal);

        public override void AddDataModules()
        {
            base.AddDataModules();
            this.DataModules.TryAddUnique<Data_Thermal>(this._dataThermal, out this._dataThermal);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            UpdatePAMVisibility();
        }

        private void UpdatePAMVisibility()
        {
            if (this == null || this._dataThermal == null) return;
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.TemperatureTxt, PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.EnvironmentFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.SolarFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.ReentryFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.ExhaustFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.otherFluxTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.CoolingEnergyToApplyTxt, debugMode && PartBackingMode == PartBackingModes.Flight);
        }
    }
}

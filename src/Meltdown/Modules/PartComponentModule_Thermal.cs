using KSP;
using KSP.Game;
using KSP.Iteration.UI.Binding;
using KSP.Logging;
using KSP.Sim.impl;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;

namespace Meltdown.Modules
{
    public class PartComponentModule_Thermal : PartComponentModule
    {
        protected Data_Thermal _dataThermal;

        public override Type PartBehaviourModuleType => typeof(Module_Thermal);

        private new GameInstance Game => GameManager.Instance.Game;

        public override void OnStart(double universalTime)
        {
            if (!this.DataModules.TryGetByType<Data_Thermal>(out this._dataThermal))
                GlobalLog.Error((object)("Unable to find a Data_Thermal in the PartComponentModule for " + this.Part.PartName));
            else if ((UnityEngine.Object)this.Game == (UnityEngine.Object)null || this.Game.ResourceDefinitionDatabase == null)
            {
                GlobalLog.Error((object)"Unable to find a valid game with a resource definition database");
            }
        }

        public override void OnUpdate(double universalTime, double deltaUniversalTime)
        {
            this._dataThermal.TemperatureTxt.SetValue(Units.PrintSI(this.Part.Temperature, "K", 3));

            this._dataThermal.EnvironmentFluxTxt.SetValue(Units.PrintSI(this.Part.ThermalData.EnvironmentFlux * 1000.0, "W", 3));
            this._dataThermal.SolarFluxTxt.SetValue(Units.PrintSI(this.Part.ThermalData.SolarFlux * 1000.0, "W", 3));
            this._dataThermal.ReentryFluxTxt.SetValue(Units.PrintSI(this.Part.ThermalData.ReentryFlux * 1000.0, "W", 3));
            this._dataThermal.ExhaustFluxTxt.SetValue(Units.PrintSI(this.Part.ThermalData.ExhaustFlux * 1000.0, "W", 3));
            this._dataThermal.otherFluxTxt.SetValue(Units.PrintSI(this.Part.ThermalData.OtherFlux * 1000.0, "W", 3));
            
            this._dataThermal.CoolingEnergyToApplyTxt.SetValue(Units.PrintSI(this.Part.ThermalData.CoolingEnergyToApply * -100000, "W", 3)); // radiators, x100 to counteract PartComponentModule_Cooler.EnergyApplied that cannot be patched.
        }
    }
}
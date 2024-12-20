﻿using KSP;
using KSP.Game;
using KSP.Logging;
using KSP.Sim.impl;

namespace Meltdown.Modules
{
    public class PartComponentModule_Thermal : PartComponentModule
    {
        public Data_Thermal _dataThermal;

        public override Type PartBehaviourModuleType => typeof(Module_Thermal);

        private new GameInstance Game => GameManager.Instance.Game;

        public override void OnStart(double universalTime)
        {
            if (!this.DataModules.TryGetByType<Data_Thermal>(out this._dataThermal))
                GlobalLog.Error((object)("[Meltdown] Unable to find a Data_Thermal in the PartComponentModule for " + this.Part.PartName));
            else if ((UnityEngine.Object)this.Game == (UnityEngine.Object)null || this.Game.ResourceDefinitionDatabase == null)
            {
                GlobalLog.Error((object)"[Meltdown] Unable to find a valid game with a resource definition database");
            }
        }

        public override void OnUpdate(double universalTime, double deltaUniversalTime)
        {
            _dataThermal.TemperatureTxt.SetValue(Units.PrintSI(Part.Temperature, "K", 3) + " / " + Units.PrintSI(Part.MaxTemp, "K", 3));

            _dataThermal.EnvironmentFluxTxt.SetValue(Units.PrintSI(Part.ThermalData.EnvironmentFlux * 1000.0, "W", 3));
            _dataThermal.SolarFluxTxt.SetValue(Units.PrintSI(Part.ThermalData.SolarFlux * 1000.0, "W", 3));
            _dataThermal.ReentryFluxTxt.SetValue(Units.PrintSI(Part.ThermalData.ReentryFlux * 1000.0, "W", 3));
            _dataThermal.ExhaustFluxTxt.SetValue(Units.PrintSI(Part.ThermalData.ExhaustFlux * 1000.0, "W", 3));
            _dataThermal.otherFluxTxt.SetValue(Units.PrintSI(Part.ThermalData.OtherFlux * 1000.0, "W", 3));
            
            _dataThermal.CoolingEnergyToApplyTxt.SetValue(Units.PrintSI(_dataThermal.energyRemoved * -1000.0, "W", 3)); // radiators

            //if (Part != null)
              //  _dataThermal.ThermalMass = Part.ThermalMass;
        }

        /**
         * Specify the heat generated by the part, in kW.
         **/
        public void SetFlux(double rate)
        {
            if (Part == null || Part.ThermalData.Equals(null))
            {
                GlobalLog.Error((object)("[Meltdown] Unable to find the part to heat."));
                return;
            }
            //if (!_dataThermal.isHeating)
            //{
            //    GlobalLog.Error((object)"[Meltdown] " + Part.PartName + " You're trying to set up a heating flux to a part not cleared for heating. Please set 'isHeating' to true first.");
            //    return;
            //}
            Part.ThermalData.OtherFlux = _dataThermal.FluxGenerated * rate;
            _dataThermal.HeatGeneratedTxt.SetValue(_dataThermal.FluxGenerated * rate);
        }
    }
}
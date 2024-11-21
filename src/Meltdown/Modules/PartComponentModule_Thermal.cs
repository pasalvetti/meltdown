using KSP.Game;
using KSP.Logging;
using KSP.Sim.impl;

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
            this._dataThermal.TemperatureTxt.SetValue(this.Part.Temperature.ToString());
        }
    }
}

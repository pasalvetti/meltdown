using KSP.Game;
using KSP.Logging;
using KSP.Sim.impl;

namespace Meltdown.Modules
{
    public class PartComponentModule_ActiveRadiator_MeltdownPlugin : PartComponentModule
    {
        public Data_ActiveRadiator_MeltdownPlugin _dataActiveRadiator;

        public override Type PartBehaviourModuleType => typeof(Module_ActiveRadiator_MeltdownPlugin);

        private new GameInstance Game => GameManager.Instance.Game;

        public override void OnStart(double universalTime)
        {
            if (!this.DataModules.TryGetByType<Data_ActiveRadiator_MeltdownPlugin>(out this._dataActiveRadiator))
                GlobalLog.Error((object)("[Meltdown] Unable to find a Data_ActiveRadiator_MeltdownPlugin in the PartComponentModule for " + this.Part.PartName));
            else if ((UnityEngine.Object)this.Game == (UnityEngine.Object)null || this.Game.ResourceDefinitionDatabase == null)
            {
                GlobalLog.Error((object)"[Meltdown] Unable to find a valid game with a resource definition database");
            }
            //if (this.Part.TryGetModule<PartComponentModule_ActiveRadiator>(out PartComponentModule_ActiveRadiator activeRadiatorComponent) && activeRadiatorComponent.dataCooler != null)
            //{
            //    _dataActiveRadiator.FluxRemoved = activeRadiatorComponent.dataCooler.fluxRemoved;
            //} else
            //{
            //    System.Diagnostics.Debug.Write("[Meltdown] Unable to get Active Radiator.");
            //}
        }
    }
}
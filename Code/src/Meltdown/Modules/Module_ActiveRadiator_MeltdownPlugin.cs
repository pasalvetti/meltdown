using KSP.Sim.Definitions;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Module_ActiveRadiator_MeltdownPlugin : PartBehaviourModule
    {
        [SerializeField]
        protected Data_ActiveRadiator_MeltdownPlugin _dataActiveRadiator;

        public override Type PartComponentModuleType => typeof(PartComponentModule_ActiveRadiator_MeltdownPlugin);

        public override void AddDataModules()
        {
            base.AddDataModules();
            this.DataModules.TryAddUnique<Data_ActiveRadiator_MeltdownPlugin>(_dataActiveRadiator, out _dataActiveRadiator);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
        }
    }
}

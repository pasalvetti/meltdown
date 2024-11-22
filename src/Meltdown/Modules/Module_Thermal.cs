using KSP.Sim.Definitions;
using UnityEngine.Serialization;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Module_Thermal : PartBehaviourModule
    {
        [FormerlySerializedAs("DataThermal")]
        [SerializeField]
        protected Data_Thermal _dataThermal;

        public override Type PartComponentModuleType => typeof(PartComponentModule_Thermal);

        public override void OnInitialize()
        {
            base.OnInitialize();
            this.UpdatePAMVisibility();
        }

        private void UpdatePAMVisibility()
        {
            if (this == null || this._dataThermal == null) return;
            this._dataThermal.SetVisible((IModuleDataContext)this._dataThermal.TemperatureTxt, this.PartBackingMode == PartBehaviourModule.PartBackingModes.Flight);
        }
    }
}

using KSP.Sim.impl;
using Meltdown.Modules;

namespace Meltdown
{
    internal class MeltdownPlugin
    {
        /**
         * Sets the 'isHeating' flag, used to mark the part as heating or not when calculating the radiators' influence.
         * Also sets the flux (in kW). If no flux is to be set, use usePatchedFlux=false (only to be used if the stock game already generates the flux).
         **/
        public static double GenerateFlux(PartComponentModule __instance, bool isHeating, double rate, bool usePatchedFlux)
        {
            if (rate == 0.0) isHeating = false;
            if (isHeating == false) rate = 0.0;
            if (__instance.Part.TryGetModule<PartComponentModule_Thermal>(out PartComponentModule_Thermal thermalComponent) && thermalComponent._dataThermal != null)
            {
                thermalComponent._dataThermal.isHeating = isHeating;
                if (usePatchedFlux)
                {
                    thermalComponent.SetFlux(rate);
                }
                //System.Diagnostics.Debug.Write("[Meltdown] GenerateFlux: FluxGenerated=" + thermalComponent._dataThermal.FluxGenerated);
                return thermalComponent._dataThermal.FluxGenerated * rate;
            }
            System.Diagnostics.Debug.Write("[Meltdown] GenerateFlux: unable to find Thermal Componant for " + __instance.Part.PartName + "/" + __instance.Part.GlobalId);
            return 0.0;
        }
    }
}

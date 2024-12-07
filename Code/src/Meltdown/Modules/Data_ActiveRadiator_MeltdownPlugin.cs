using I2.Loc;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using UnityEngine;

namespace Meltdown.Modules
{
    public class Data_ActiveRadiator_MeltdownPlugin : ModuleData
    {
        public override Type ModuleType => typeof(Module_ActiveRadiator_MeltdownPlugin);

        [KSPDefinition]
        public List<PartModuleResourceSetting> RequiredResources;
        [KSPDefinition]
        public double FluxRemoved;

        public override List<OABPartData.PartInfoModuleEntry> GetPartInfoEntries(Type partBehaviourModuleType, List<OABPartData.PartInfoModuleEntry> delegateList)
        {
            if (partBehaviourModuleType == this.ModuleType)
            {
                delegateList.Add(new OABPartData.PartInfoModuleEntry(LocalizationManager.GetTranslation("PartModules/Generic/Tooltip/Resources", true, 0, true, false, (GameObject)null, (string)null, true), new OABPartData.PartInfoModuleMultipleEntryValueDelegate(this.GetResourceStrings)));
                delegateList.Add(new OABPartData.PartInfoModuleEntry(LocalizationManager.GetTranslation("PartModules/Thermal/HeatRemoved", true, 0, true, false, (GameObject)null, (string)null, true), new OABPartData.PartInfoModuleMultipleEntryValueDelegate(this.GetHeatRemovedString)));
            }
            return delegateList;
        }

        /**
         * Displays the EC consumption in the OAB part entries.
         **/
        private List<OABPartData.PartInfoModuleSubEntry> GetResourceStrings(OABPartData.OABSituationStats oabSituationStats)
        {
            List<OABPartData.PartInfoModuleSubEntry> resourceStrings = [];
            for (int index = 0; index < this.RequiredResources.Count<PartModuleResourceSetting>(); ++index)
            {
                ResourceDefinitionData definitionData = ModuleData.Game.ResourceDefinitionDatabase.GetDefinitionData(ModuleData.Game.ResourceDefinitionDatabase.GetResourceIDFromName(this.RequiredResources[index].ResourceName));
                string title = LocalizationManager.GetTranslation("PartModules/Generic/Tooltip/ResourceRateMax", true, 0, true, false, (GameObject)null, (string)null, true);
                object resourceRate = (object)PartModuleTooltipLocalization.FormatResourceRate((double)this.RequiredResources[index].Rate, PartModuleTooltipLocalization.GetTooltipResourceUnits(RequiredResources[index].ResourceName));
                resourceStrings.Add(new OABPartData.PartInfoModuleSubEntry(string.Format(title, (object)definitionData.DisplayName, resourceRate)));
            }
            return resourceStrings;
        }

        /**
         * Displays the heat generated in the OAB part entries.
         **/
        private List<OABPartData.PartInfoModuleSubEntry> GetHeatRemovedString(OABPartData.OABSituationStats oabSituationStats)
        {
            return
            [
                new OABPartData.PartInfoModuleSubEntry(FluxRemoved + " kW")
            ];
        }

    }
}

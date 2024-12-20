using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using Meltdown.Patches;
using SpaceWarp;
using SpaceWarp.API.Mods;

namespace Meltdown;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class Meltdown : BaseSpaceWarpPlugin
{
    // Useful in case some other mod wants to use this mod a dependency
    [PublicAPI] public const string ModGuid = MyPluginInfo.PLUGIN_GUID;
    [PublicAPI] public const string ModName = MyPluginInfo.PLUGIN_NAME;
    [PublicAPI] public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    /// <summary>
    /// Runs when the mod is first initialized.
    /// </summary>
    public override void OnInitialized()
    {
        base.OnInitialized();

        // Register all Harmony patches in the project
        Harmony.CreateAndPatchAll(typeof(MeltdownPlugin));
        Harmony.CreateAndPatchAll(typeof(ActiveRadiatorPatches));
        Harmony.CreateAndPatchAll(typeof(CommandPatches));
        Harmony.CreateAndPatchAll(typeof(CoolerPatches));
        Harmony.CreateAndPatchAll(typeof(GeneratorPatches));
        Harmony.CreateAndPatchAll(typeof(HeatshieldPatches));
        Harmony.CreateAndPatchAll(typeof(ResourceConverterPatches));
        Harmony.CreateAndPatchAll(typeof(SolarPanelPatches));
        Harmony.CreateAndPatchAll(typeof(ThermalComponentPatches));
    }
}


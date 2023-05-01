using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace ExtremeVoiceEngine;

[BepInAutoPlugin("me.yukieiji.extremevoiceengine", "Extreme Voice Engine")]
[BepInDependency(
    ExtremeRoles.ExtremeRolesPlugin.Id,
    BepInDependency.DependencyFlags.HardDependency)] // Never change it!
[BepInProcess("Among Us.exe")]
public partial class ExtremeVoiceEnginePlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(Id);

    public override void Load()
    {
        AddComponent<VoiceEngine>();
    }
}
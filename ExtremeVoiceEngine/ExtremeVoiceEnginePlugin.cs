using BepInEx;
using BepInEx.Logging;
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

#pragma warning disable CS8618
    public static ExtremeVoiceEnginePlugin Instance { get; private set; }
    public static ManualLogSource Logger { get; private set; }
#pragma warning restore CS8618

    public override void Load()
    {
        Instance = this;
        Logger = Log;

        Harmony.PatchAll();

        AddComponent<VoiceEngine>();

        if (VoiceEngine.Instance != null)
        {
            VoiceEngine.CreateCommand();

            var engine = new VoiceVox.VoiceVoxEngine();
            engine.SetParameter(new VoiceVox.VoiceVoxParameter());
            engine.Wait = 2.0f;

            VoiceEngine.Instance.Engine = engine;
        }
    }
}
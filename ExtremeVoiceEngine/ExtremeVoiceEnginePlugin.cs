using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using ExtremeRoles.Module;

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

        var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
        Updater.Instance.AddMod<ExRRepositoryInfo>($"{assembly?.GetName().Name}.dll");
        Il2CppRegisterAttribute.Registration(assembly);
    }
}
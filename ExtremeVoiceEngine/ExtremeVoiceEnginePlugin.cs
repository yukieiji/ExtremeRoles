using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using ExtremeRoles.Module;
using ExtremeRoles.Core;
using ExtremeRoles.Core.Service;

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

    private string? assemblyName = string.Empty;
    private System.Version? version = null;

    public override void Load()
    {
		if (ExtremeRoles.ExtremeRolesPlugin.DebugMode == null)
		{
			AutoModInstaller.Instance.AddMod<ExRRepositoryInfo>();
			return;
		}

		Instance = this;
        Logger = Log;

        Harmony.PatchAll();

        AddComponent<VoiceEngine>();
		AutoModInstaller.Instance.AddMod<ExRRepositoryInfo>();

		var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
        this.assemblyName = assembly?.GetName().Name;
        this.version = assembly?.GetName().Version;
        Il2CppRegisterAttribute.Registration(assembly);

		ExtremeRoles.Translation.TranslatorManager.Register<Translator>();
		StatusTextShower.Instance.Add(() => this.ToString());
	}

    public override string ToString()
        => $"{this.assemblyName} - v{this.version}";
}
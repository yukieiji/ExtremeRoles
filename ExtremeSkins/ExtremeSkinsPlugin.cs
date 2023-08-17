using System.Net.Http;

using BepInEx;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using ExtremeRoles.Module;

using ExtremeSkins.SkinManager;
using ExtremeSkins.Module.ApiHandler;
using ExtremeSkins.Module.ApiHandler.ExtremeHat;
using ExtremeSkins.Module.ApiHandler.ExtremeVisor;

namespace ExtremeSkins;

[BepInAutoPlugin("me.yukieiji.extremeskins", "Extreme Skins")]
[BepInDependency(
    ExtremeRoles.ExtremeRolesPlugin.Id,
    BepInDependency.DependencyFlags.HardDependency)] // Never change it!
[BepInProcess("Among Us.exe")]
public partial class ExtremeSkinsPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(Id);

#pragma warning disable CS8618
	public static ExtremeSkinsPlugin Instance;
	internal static BepInEx.Logging.ManualLogSource Logger;
#pragma warning restore CS8618

	public const string SkinComitCategory = "SkinComit";

    public override void Load()
    {
        Helper.Translation.LoadTransData();

        Logger = Log;

        Instance = this;

#if WITHHAT
        ExtremeHatManager.Initialize();
#endif
#if WITHNAMEPLATE
        ExtremeNamePlateManager.Initialize();
#endif
#if WITHVISOR
        ExtremeVisorManager.Initialize();
#endif

        CreatorModeManager.Initialize();

        ExtremeColorManager.Initialize();

        VersionManager.PlayerVersion.Clear();

        Harmony.PatchAll();

		if (CreatorModeManager.Instance.IsEnable)
		{
#if WITHHAT
			ApiServer.Register("/exs/hat/"   , HttpMethod.Get , new GetHatHandler());
			ApiServer.Register("/exs/hat/"   , HttpMethod.Put , new PutHatHandler());
			ApiServer.Register("/exs/hat/"   , HttpMethod.Post, new PostNewHatHandler());
#endif
#if WITHHAT
			ApiServer.Register("/exs/visor/", HttpMethod.Get , new GetVisorHandler());
			ApiServer.Register("/exs/visor/", HttpMethod.Put , new PutVisorHandler());
			ApiServer.Register("/exs/visor/", HttpMethod.Post, new PostNewVisorHandler());
#endif
			ApiServer.Register("/exs/status/", HttpMethod.Get , new GetStatusHandler());
		}

        var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());

		if (assembly is null) { return; }

        Updater.Instance.AddMod<ExRRepositoryInfo>($"{assembly.GetName().Name}.dll");
        Il2CppRegisterAttribute.Registration(assembly);
    }
}

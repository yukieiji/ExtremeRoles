using System.Net.Http;

using BepInEx;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using ExtremeRoles.Module;

using ExtremeSkins.Loader;
using ExtremeSkins.Module;
using ExtremeSkins.Module.ApiHandler;
using ExtremeSkins.Module.ApiHandler.ExtremeHat;
using ExtremeSkins.Module.ApiHandler.ExtremeVisor;
using ExtremeSkins.Module.ApiHandler.ExtremeNamePlate;

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
		if (ExtremeRoles.ExtremeRolesPlugin.DebugMode == null)
		{
			AutoModInstaller.Instance.AddMod<ExRRepositoryInfo>();
			return;
		}

        Helper.Translation.LoadTransData();

        Logger = Log;

        Instance = this;

#if WITHHAT
		ExtremeCosmicLoader.Instance.AddLoader<CustomHat, HatLoader>();
#endif
#if WITHNAMEPLATE
		ExtremeCosmicLoader.Instance.AddLoader<CustomNamePlate, NamePlateLoader>();
#endif
#if WITHVISOR
		ExtremeCosmicLoader.Instance.AddLoader<CustomVisor, VisorLoader>();
#endif

		CreatorModeManager.Initialize();

		CustomColorLoader.StaticLoad();

        VersionManager.PlayerVersion.Clear();

        Harmony.PatchAll();

		if (CreatorModeManager.Instance.IsEnable)
		{
			ApiServer.Register("/exs/"       , HttpMethod.Get , new GetStatusHandler());
#if WITHHAT
			ApiServer.Register("/exs/hat/", HttpMethod.Get , new GetHatHandler());
			ApiServer.Register("/exs/hat/", HttpMethod.Put , new PutHatHandler());
			ApiServer.Register("/exs/hat/", HttpMethod.Post, new PostNewHatHandler());
#endif
#if WITHVISOR
			ApiServer.Register("/exs/visor/", HttpMethod.Get , new GetVisorHandler());
			ApiServer.Register("/exs/visor/", HttpMethod.Put , new PutVisorHandler());
			ApiServer.Register("/exs/visor/", HttpMethod.Post, new PostNewVisorHandler());
#endif
#if WITHVISOR
			ApiServer.Register("/exs/nameplate/", HttpMethod.Get , new GetNamePlateHandler());
			ApiServer.Register("/exs/nameplate/", HttpMethod.Put , new PutNamePlateHandler());
			ApiServer.Register("/exs/nameplate/", HttpMethod.Post, new PostNewNamePlateHandler());
#endif
		}

		StatusTextShower.Instance.Add(() => CreatorModeManager.Instance.StatusString);
		AutoModInstaller.Instance.AddMod<ExRRepositoryInfo>();

		var assembly = System.Reflection.Assembly.GetAssembly(this.GetType());
		if (assembly is null) { return; }
        Il2CppRegisterAttribute.Registration(assembly);
    }
}

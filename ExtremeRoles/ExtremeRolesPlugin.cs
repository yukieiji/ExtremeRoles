global using ExtremeRoles.Module.CustomOption;
global using Tr = ExtremeRoles.Extension.Controller.TranslationControllerExtension;
global using InfoOverlay = ExtremeRoles.Module.InfoOverlay.Controller;
global using ExRError = ExtremeRoles.Module.ErrorCode<ExtremeRoles.ErrorCode>;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using System.Net.Http;

using ExtremeRoles.Compat;
using ExtremeRoles.Module;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.ApiHandler;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Migrator;
using ExtremeRoles.Translation;

namespace ExtremeRoles;

[BepInAutoPlugin("me.yukieiji.extremeroles", "Extreme Roles")]
[BepInDependency(
    "gg.reactor.api",
    BepInDependency.DependencyFlags.SoftDependency)] // Reactorとのパッチの兼ね合いで入れておく
[BepInDependency(
    Compat.ModIntegrator.SubmergedIntegrator.Guid,
    BepInDependency.DependencyFlags.SoftDependency)]
[BepInProcess("Among Us.exe")]
public partial class ExtremeRolesPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new Harmony(Id);

    public static ExtremeRolesPlugin Instance { get; private set; }
    public static ExtremeShipStatus ShipState { get; private set; }

	internal static BepInEx.Logging.ManualLogSource Logger;

    public static ConfigEntry<bool> DebugMode { get; private set; }
    public static ConfigEntry<bool> IgnoreOverrideConsoleDisable { get; private set; }

	public ExtremeRolesPlugin() : base()
	{
		Instance = this;
		Logger = Log;

		if (MigratorManager.IsMigrate(this.Config, out int version))
		{
			MigratorManager.MigrateConfig(this.Config, version);
		}
	}

    public override void Load()
    {
		try
		{
			normalBoot();
		}
		catch(System.Exception ex)
		{
			Logger.LogError($"ExR can't boot with normal ops\nError:{ex.Message}");
			Logger.LogWarning("Try boot ExR with SafeMode");
			Compat.SafeBoot.SafeBootScheduler.Boot(this.Harmony);

			DebugMode = null;
		}
    }

	private void normalBoot()
	{
		ShipState = new ExtremeShipStatus();

		DebugMode = Config.Bind("DeBug", "DebugMode", false);
		IgnoreOverrideConsoleDisable = Config.Bind(
			"DeBug", "IgnoreOverrideConsoleDisable", false,
			"If enabled, will ignore force disabling BepInEx.Console");

		this.Harmony.PatchAll();

		CompatModManager.Initialize();

		OptionCreator.Create();

		AddComponent<ExtremeRolePluginBehavior>();
		AddComponent<UnityMainThreadDispatcher>();

		if (BepInExUpdater.IsUpdateRquire())
		{
			AddComponent<BepInExUpdater>();
		}

		ApiServer.Register("/au/chat/", HttpMethod.Get, new GetChat());
		ApiServer.Register(PostChat.Path, HttpMethod.Post, new PostChat());
		ApiServer.Register(ChatWebUI.Path, HttpMethod.Get, new OpenChatWebUi());

		var assm = System.Reflection.Assembly.GetAssembly(this.GetType());

        Il2CppRegisterAttribute.Registration(assm);

		StatusTextShower.Instance.Add(() => PublicBeta.Instance.CurStateString);

		UnityObjectLoader.LoadCommonAsset();

		ExtremeSystemTypeManager.ModInitialize();
		VersionChecker.RegisterAssembly(assm, 0);
		TranslatorManager.Register<Translator>();
	}
}

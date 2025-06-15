global using ExtremeRoles.Module.CustomOption;
global using Tr = ExtremeRoles.Extension.Controller.TranslationControllerExtension;
global using InfoOverlay = ExtremeRoles.Module.InfoOverlay.Controller;
global using ExRError = ExtremeRoles.Module.ErrorCode<ExtremeRoles.ErrorCode>;

global using NormalGameOption = AmongUs.GameOptions.NormalGameOptionsV09;
global using HnSGameOption = AmongUs.GameOptions.HideNSeekGameOptionsV09;

using System;
using System.Net.Http;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Compat;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Migrator;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.ScreenManagerHook;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.ApiHandler;
using ExtremeRoles.Resources;
using ExtremeRoles.Translation;

namespace ExtremeRoles;

[BepInAutoPlugin("me.yukieiji.extremeroles", "Extreme Roles")]
[BepInDependency(
    "gg.reactor.api",
    BepInDependency.DependencyFlags.SoftDependency)] // Reactorとのパッチの兼ね合いで入れておく
[BepInDependency(
    Compat.ModIntegrator.SubmergedIntegrator.Guid,
    BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(
	Compat.ModIntegrator.CrowdedMod.Guid,
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

	public HttpClient Http { get; } = new HttpClient();
	public IServiceProvider Provider { get; }

	public ExtremeRolesPlugin() : base()
	{
		Instance = this;
		Logger = Log;

		if (MigratorManager.IsMigrate(this.Config, out int version))
		{
			MigratorManager.MigrateConfig(this.Config, version);
		}

		this.Http.DefaultRequestHeaders.Add(
			"User-Agent", "ExtremeRoles");
		this.Http.DefaultRequestHeaders.CacheControl = new()
		{
			NoCache = true
		};

		var collection = new ServiceCollection();

		collection.AddTransient<IRoleAssignee, ExtremeRoleAssignee>();
		collection.AddTransient<IVanillaRoleProvider, VanillaRoleProvider>();
		collection.AddTransient<IRoleAssignDataBuilder, ExtremeRoleAssignDataBuilder>();
		collection.AddTransient<ISpawnLimiter, ExtremeSpawnLimiter>();
		collection.AddTransient<IRoleAssignDataPreparer, ExtremeRoleAssginDataPreparer>();
		collection.AddTransient<ISpawnDataManager, RoleSpawnDataManager>();

		collection.AddTransient<IRoleAssignDataBuildBehaviour, CombinationRoleAssignDataBuilder>();
		collection.AddTransient<IRoleAssignDataBuildBehaviour, SingleRoleAssignDataBuilder>();
		collection.AddTransient<IRoleAssignDataBuildBehaviour, NotAssignedPlayerAssignDataBuilder>();

		collection.AddTransient<PlayerRoleAssignData>();

		// 追加ここから
		// IAssignFilterInitializer とその実装を登録
		collection.AddTransient<IAssignFilterInitializer, AssignFilterInitializer>(); // AssignFilterInitializer の using が必要になる場合がある

		// RoleDependencyRule のリストを準備 (具体的なルールはユーザーが後で定義・追加する必要がある)
		var dependencyRules = new System.Collections.Generic.List<RoleDependencyRule> // RoleDependencyRule の using が必要になる場合がある
		{
			// --- 具体的なルール定義の例 (実際の役職IDや条件に合わせてユーザーが実装) ---
			// 例: 役職A (ID: 1001) が存在し、役職B (ID: 1002) が存在せず、
			//     役職Aのインスタンスの特定のboolプロパティ OptionX が true の場合。
			// new RoleDependencyRule(
			//    (ExtremeRoleId)1001,
			//    (ExtremeRoleId)1002,
			//    roleAInstance =>
			//    {
			//        // roleAInstance を実際の型にキャストしてプロパティを確認
			//        // if (roleAInstance is ConcreteRoleA concreteA) return concreteA.OptionX;
			//        return false; // ここは実際の条件に置き換える
			//    }
			// ),
			// --- 他の依存関係ルールをここに追加 ---
		};
		collection.AddSingleton<System.Collections.Generic.IEnumerable<RoleDependencyRule>>(dependencyRules);

		// IRoleAssignValidator とその実装を登録
		collection.AddTransient<IRoleAssignValidator, RoleAssignValidator>(); // RoleAssignValidator の using が必要になる場合がある
		// 追加ここまで

		Provider = collection.BuildServiceProvider();
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
		ApiServer.Register(ConectGame.Path, HttpMethod.Get, new ConectGame());

		ScreenManagerHookcs.RegisterLoad();

		var assm = System.Reflection.Assembly.GetAssembly(this.GetType());

        Il2CppRegisterAttribute.Registration(assm);

		UnityObjectLoader.LoadCommonAsset();

		ExtremeSystemTypeManager.ModInitialize();
		VersionChecker.RegisterAssembly(assm, 0);
		TranslatorManager.Register<Translator>();
	}
}

using ExtremeRoles.Compat;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Core;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Module;
using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;
using ExtremeRoles.Module.RoleAssign.RoleAssignDataChecker;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Patches;
using ExtremeRoles.Patches.Controller;
using ExtremeRoles.Patches.MapOverlay;
using ExtremeRoles.Patches.Meeting;
using ExtremeRoles.Patches.Player;
using ExtremeRoles.Patches.Role;
using ExtremeRoles.Patches.Ship;
using ExtremeRoles.Roles.Solo.Liberal;
using Microsoft.Extensions.DependencyInjection;
using System;


namespace ExtremeRoles;

public partial class ExtremeRolesPlugin
{
	public static IServiceProvider BuildProvider()
	{
		var collection = new ServiceCollection();

		collection
			.AddSingleton<IIl2CppObjectProvider, DefaultIl2CppObjectProvider>();

		collection
			.AddSingleton<IModLogger, BepInExLogger>();

		collection
			.AddTransient<IPluginLoader, BepInExPluginLoader>()
			.AddTransient<IAccessTool, AccessToolWrapper>()
			.AddTransient<IHarmonyPatchProvider, HarmonyPatchProvider>()
			.AddTransient<IModInitializerFactory, ModInitializerFactory>()
			.AddSingleton<CompatModManager>();

		collection
			.AddSingleton<IGameProgress, GameProgress>()
			.AddScoped<INomalGameRoleContainer, NormalGameRoleContainer>();

		collection
			.AddSingleton<GameRuntime>()
			.AddSingleton<IGameRuntime>(x => x.GetRequiredService<GameRuntime>())
			.AddSingleton<IGameRuntimeStarter>(x => x.GetRequiredService<GameRuntime>())
			.AddSingleton<IGameRuntimeEnder>(x => x.GetRequiredService<GameRuntime>());

		collection
			.AddTransient<IRoleAssignee, ExtremeRoleAssignee>()
			.AddTransient<IVanillaRoleProvider, VanillaRoleProvider>()
			.AddTransient<ISpawnLimiter, ExtremeSpawnLimiter>()
			.AddTransient<IRoleAssignDataPreparer, ExtremeRoleAssginDataPreparer>()
			.AddTransient<ISpawnDataManager, RoleSpawnDataManager>();

		collection
			.AddScoped<VanillaRolePlayerOption>()
			.AddScoped<IVanillaRolePlayerAssignDataProvider, VanillaRolePlayerAssignDataProviderSelector>()

			.AddTransient<DefaultVanillaRolePlayerAssignDataProvider>()
			.AddTransient<MockVanillaRolePlayerAssignDataProvider>();

		collection
			.AddTransient<ISingleRoleAssignHelper, SingleRoleAssignHelper>()
			.AddTransient<IImpostorSingleRoleAssignDataBuilder, ImpostorSingleRoleAssignDataBuilder>()
			.AddTransient<INeutralSingleRoleAssignDataBuilder, NeutralSingleRoleAssignDataBuilder>()
			.AddTransient<ILiberalSingleRoleAssignDataBuilder, LiberalSingleRoleAssignDataBuilder>()
			.AddTransient<ICrewmateSingleRoleAssignDataBuilder, CrewmateSingleRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuilder, ExtremeRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, CombinationRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, SingleRoleAssignDataBuilder>()
			.AddTransient<IRoleAssignDataBuildBehaviour, NotAssignedPlayerAssignDataBuilder>();

		collection.AddTransient<PlayerRoleAssignData>();

		// 追加ここから
		// IAssignFilterInitializer とその実装を登録
		collection.AddTransient<IAssignFilterInitializer, AssignFilterInitializer>();

		// IRoleAssignValidator とその実装を登録
		collection
			.AddTransient<IRoleAssignValidator, RoleAssignValidator>()

			.AddTransient<IRoleAssignDataChecker, RoleAssignDependencyChecker>()
			.AddTransient<IRoleDependencyRuleFactory, RoleDependencyRuleFactory>()

			.AddTransient<IRoleProvider, RoleProvider>();

		// Liberal
		collection
			.AddSingleton<LiberalDefaultOptionLoader>()
			.AddTransient(
				x => ExtremeSystemTypeManager.Instance.CreateOrGet(ExtremeSystemType.LiberalMoneyBank, () =>
				{
					var option = x.GetRequiredService<LiberalDefaultOptionLoader>();
					return new LiberalMoneyBankSystem(option);
				})
			)
			.AddTransient(x =>
			{
				var option = x.GetRequiredService<LiberalDefaultOptionLoader>();
				return new LeaderCoreOption(option);
			})
			.AddTransient<LeaderVisual>()
			.AddTransient<LeaderAbilityHandler>()
			.AddTransient<LeaderStatus>()
			.AddTransient<Leader>()
			.AddTransient<DoveCommonAbilityHandler>()
			.AddTransient<Dove>()
			.AddTransient<Militant>()
			.AddTransient<Encloser>();

		collection.AddTransient<ExtremeGameEndChecker>();

		// EventManager
		collection.AddSingleton<IEventManager, Module.Event.EventManager>();

		collection.AddTransient<ICustomRegionProvider, DefaultCustomRegionProvider>();

		RegisterPatchService(collection);

		// シングルトン対策として、ExtremeSystemTypeManagerのインスタンスをシングルトンとして登録(後にDIへ完全移行させる)
		collection.AddSingleton(x =>
		{
			var mng = ExtremeSystemTypeManager.Instance;
			return mng;
		});
		return collection.BuildServiceProvider();
	}

	private static void RegisterPatchService(IServiceCollection collection)
	{
		collection
			.AddSingleton<GameDataRecomputeTaskCountsPatchBody>()

			.AddSingleton<ProgressTrackerFixedUpdatePatchBody>()
			.AddSingleton<PlayerPhysicsFixedUpdatePatchBody>()

			.AddSingleton<PlayerControlSetKillTimerPatchBody>()
			.AddSingleton<PlayerControlShapeshiftPatchBody>()

			.AddSingleton<ChatControllerAddChatPatchBody>()

			.AddSingleton<MapCountOverlayUpdatePatchBody>()
			.AddSingleton<CounterAreaUpdatePatchBody>()

			.AddSingleton<ShipStatusOnEnablePatchBody>()
			.AddSingleton<ShipStatusPrespawnStepPatchBody>()

			.AddSingleton<RoleBehaviourGetAbilityDistancePatchBody>()
			.AddSingleton<RoleBehaviourIsValidTargetPatchBody>()

			.AddSingleton<PlayerVoteAreaSelectPatchBody>()

			.AddSingleton<IntroCutScenceBeginPatch>()
			.AddSingleton<IntroCutScenceCoBeginPatchBody>()
			.AddSingleton<IntroCutScenceShowRolePatchBody>();
	}
}

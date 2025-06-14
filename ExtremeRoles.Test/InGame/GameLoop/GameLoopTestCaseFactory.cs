using AmongUs.GameOptions;

using ExtremeRoles.Roles;

using ExtremeRoles.Test.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Test.InGame.GameLoop;

public class GameLoopTestCaseFactory
{
	public GameLoopTestCase[] Get()
		=>
#if DEBUG
		[
			new("Random", 128),
			new("IRoleAbilityRole", 2,
			[
				ExtremeRoleId.Carpenter,
				ExtremeRoleId.BodyGuard,
				ExtremeRoleId.SandWorm,
			]),
			new("IRoleAutoBuildAbilityRole", 2,
			[
				ExtremeRoleId.Hatter,
				ExtremeRoleId.Eater,
				ExtremeRoleId.Carrier,
				ExtremeRoleId.Thief,
				ExtremeRoleId.Traitor,
				ExtremeRoleId.Teleporter,
				ExtremeRoleId.Supervisor,
				ExtremeRoleId.Psychic,
				ExtremeRoleId.Mover,
			]),
			new("NeutralRemove", 2,
			[
				ExtremeRoleId.Jester, ExtremeRoleId.TaskMaster,
				ExtremeRoleId.Neet, ExtremeRoleId.Umbrer,
				ExtremeRoleId.Madmate
			]),
			new("YokoWin", 2, [ ExtremeRoleId.Yoko ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
			}),
			new("NeutralWin", 10,
			[
				ExtremeRoleId.Alice, ExtremeRoleId.Jackal,
				ExtremeRoleId.Missionary, ExtremeRoleId.Miner,
				ExtremeRoleId.Eater, ExtremeRoleId.Queen
			],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 0));
			}),
			new("QueenWin", 10, [ ExtremeRoleId.Queen ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("QueenWithPawn", 2, [ ExtremeRoleId.Queen, ExtremeRoleId.Pawn ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("QueenWithKnight", 2, [ ExtremeRoleId.Queen, ExtremeRoleId.Knight ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("QueenWithKnightAndPawn", 2, [ ExtremeRoleId.Queen, ExtremeRoleId.Knight, ExtremeRoleId.Pawn ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("YandereWin", 10, [ ExtremeRoleId.Yandere ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("YandereWinWithIntimate", 2, [ ExtremeRoleId.Yandere, ExtremeRoleId.Intimate ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("YandereWinWithSurrogator", 2, [ ExtremeRoleId.Yandere, ExtremeRoleId.Surrogator ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("YandereWinWithSurrogatorAndIntimate", 2, [ ExtremeRoleId.Yandere, ExtremeRoleId.Intimate, ExtremeRoleId.Surrogator ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("JackalWinWithFurry", 2, [ ExtremeRoleId.Jackal, ExtremeRoleId.Furry ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("JackalWinWithShepherd", 2, [ExtremeRoleId.Jackal, ExtremeRoleId.Shepherd ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("JackalWinWithFurryAndShepherd", 2, [ExtremeRoleId.Jackal, ExtremeRoleId.Furry, ExtremeRoleId.Shepherd ], () =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				GameUtility.UpdateAmongUsOption(
					new RequireOption<Int32OptionNames, int>(
						Int32OptionNames.NumImpostors, 3));
			}),
			new("MagicanTeleport", 10, PreTestCase: MagicianTeleportTest.Test),
		];
#elif RELEASE
		[
			new("Random", 5),
		];
#endif
}

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
			new("Random", 256),
			new("IRoleAbilityRole", 5,
			[
				ExtremeRoleId.Carpenter,
				ExtremeRoleId.BodyGuard,
				ExtremeRoleId.SandWorm,
			]),
			new("IRoleAutoBuildAbilityRole", 5,
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
			new("NeutralRemove", 5,
			[
				ExtremeRoleId.Jester, ExtremeRoleId.TaskMaster,
				ExtremeRoleId.Neet, ExtremeRoleId.Umbrer,
				ExtremeRoleId.Madmate
			]),
			new("YokoWin", 5, [ ExtremeRoleId.Yoko ],
			() =>
			{
				GameUtility.UpdateExROption(
					OptionTab.GeneralTab,
					(int)ShipGlobalOptionCategory.NeutralWinOption,
					new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
			}),
			new("NeutralWin", 100,
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
			new("QueenWin", 25, [ ExtremeRoleId.Queen ],
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
			new("YandereWin", 25, [ ExtremeRoleId.Yandere ]),
			new("MagicanTeleport", 128, PreTestCase: MagicianTeleportTest.Test),
		];
#elif RELEASE
		[
			new("Random", 5),
		];
#endif
}

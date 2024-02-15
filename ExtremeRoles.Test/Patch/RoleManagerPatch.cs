using HarmonyLib;
using AmongUs.GameOptions;

using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Test.Patches.Manager;


[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerSelectRolesPatch
{

	public static void Prefix()
	{
		if (!GameMudderEndTestingBehaviour.Enable) { return; }

		PlayerRoleAssignData assignData = PlayerRoleAssignData.Instance;

		// ダミープレイヤーは役職がアサインされてないので無理やりアサインする
		var allPlayer = assignData.NeedRoleAssignPlayer;

		var gameOption = GameOptionsManager.Instance;
		var currentOption = gameOption.CurrentGameOptions;

		int adjustedNumImpostors = currentOption.GetAdjustedNumImpostors(allPlayer.Count);

		var il2CppListPlayer = new Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo>();

		foreach (PlayerControl player in allPlayer)
		{
			il2CppListPlayer.Add(player.Data);
		}

		GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
			il2CppListPlayer, currentOption, RoleTeamTypes.Impostor,
			adjustedNumImpostors,
			new Il2CppSystem.Nullable<RoleTypes>()
			{
				value = RoleTypes.Impostor,
				has_value = true
			});
		GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(
			il2CppListPlayer, currentOption, RoleTeamTypes.Crewmate,
			int.MaxValue,
			new Il2CppSystem.Nullable<RoleTypes>()
			{
				value = RoleTypes.Crewmate,
				has_value = true
			});

		// アサイン済みにする
		foreach (PlayerControl player in allPlayer)
		{
			player.roleAssigned = true;
		}
	}
}
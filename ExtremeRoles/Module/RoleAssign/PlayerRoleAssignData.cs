using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public sealed class PlayerRoleAssignData(IVanillaRoleProvider roleProvider)
{
	public IReadOnlyList<VanillaRolePlayerAssignData> NeedRoleAssignPlayer => this.needRoleAssignPlayer;
	public IReadOnlyList<IPlayerToExRoleAssignData> Data => this.assignData;

	public int ControlId
	{
		get
		{
			int result = this.gameControlId;

			++this.gameControlId;

			return result;
		}
	}

	private readonly List<VanillaRolePlayerAssignData> needRoleAssignPlayer =
		GameData.Instance.AllPlayers.GetFastEnumerator().Select(
			x => new VanillaRolePlayerAssignData(x))
		.OrderBy(x => RandomGenerator.Instance.Next()).ToList();
	private int gameControlId = 0;

	private readonly List<IPlayerToExRoleAssignData> assignData = [];
	private readonly Dictionary<byte, ExtremeRoleType> combRoleAssignPlayerId = [];

	private readonly IReadOnlySet<RoleTypes> crewRole = roleProvider.AllCrewmate;
	private readonly IReadOnlySet<RoleTypes> impRole = roleProvider.AllImpostor;

	public IReadOnlyList<VanillaRolePlayerAssignData> GetCanImpostorAssignPlayer()
		=> this.needRoleAssignPlayer.FindAll(x => this.impRole.Contains(x.Role));

	public IReadOnlyList<VanillaRolePlayerAssignData> GetCanCrewmateAssignPlayer()
		=> this.needRoleAssignPlayer.FindAll(x => this.crewRole.Contains(x.Role));

	public bool TryGetCombRoleAssign(byte playerId, out ExtremeRoleType team)
		=> this.combRoleAssignPlayerId.TryGetValue(playerId, out team);

	public void AddCombRoleAssignData(
		in PlayerToCombRoleAssignData data, ExtremeRoleType team)
	{
		this.combRoleAssignPlayerId.Add(data.PlayerId, team);
		this.AddAssignData(data);
	}

	public void AddAssignData(IPlayerToExRoleAssignData data)
		=> this.assignData.Add(data);

	public void AddPlayer(in VanillaRolePlayerAssignData player)
		=> this.needRoleAssignPlayer.Add(player);

	public void RemveFromPlayerControl(PlayerControl player)
		=> this.needRoleAssignPlayer.RemoveAll(
			x =>
				x.PlayerId == player.PlayerId &&
				x.PlayerName == player.Data.DefaultOutfit.PlayerName);

	public void RemvePlayer(VanillaRolePlayerAssignData player)
		=> this.needRoleAssignPlayer.RemoveAll(x => x == player);
}
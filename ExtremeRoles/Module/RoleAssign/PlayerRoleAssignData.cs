using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public sealed class PlayerRoleAssignData
{
	public IReadOnlyList<VanillaRolePlayerAssignData> NeedRoleAssignPlayer => this.needRoleAssignPlayer;
	public IReadOnlyList<IPlayerToExRoleAssignData> AssignData => this.assignData;

	public int ControlId
	{
		get
		{
			int result = this.gameControlId;

			++this.gameControlId;

			return result;
		}
	}

	private List<VanillaRolePlayerAssignData> needRoleAssignPlayer;
	private readonly List<IPlayerToExRoleAssignData> assignData = [];
	private readonly Dictionary<byte, ExtremeRoleType> combRoleAssignPlayerId = new Dictionary<byte, ExtremeRoleType>();

	private int gameControlId = 0;

	public PlayerRoleAssignData()
	{
		this.assignData.Clear();
		this.combRoleAssignPlayerId.Clear();

		this.needRoleAssignPlayer = new List<VanillaRolePlayerAssignData>(
			GameData.Instance.AllPlayers.GetFastEnumerator().Select(
				x => new VanillaRolePlayerAssignData(x)));
		this.gameControlId = 0;
	}

	public IReadOnlyList<VanillaRolePlayerAssignData> GetCanImpostorAssignPlayer()
	{
		return this.needRoleAssignPlayer.FindAll(
			x =>
			{
				return x.Role is
					RoleTypes.Impostor or
					RoleTypes.Shapeshifter or
					RoleTypes.Phantom;
			});
	}

	public IReadOnlyList<VanillaRolePlayerAssignData> GetCanCrewmateAssignPlayer()
	{
		return this.needRoleAssignPlayer.FindAll(
			x =>
			{
				return x.Role is
					RoleTypes.Crewmate or
					RoleTypes.Engineer or
					RoleTypes.Scientist or
					RoleTypes.Noisemaker or
					RoleTypes.Tracker;
			});
	}

	public bool TryGetCombRoleAssign(byte playerId, out ExtremeRoleType team)
	{
		return this.combRoleAssignPlayerId.TryGetValue(playerId, out team);
	}

	public void AddCombRoleAssignData(
		PlayerToCombRoleAssignData data, ExtremeRoleType team)
	{
		this.combRoleAssignPlayerId.Add(data.PlayerId, team);
		this.AddAssignData(data);
	}

	public void AddAssignData(IPlayerToExRoleAssignData data)
	{
		this.assignData.Add(data);
	}

	public void AddPlayer(in VanillaRolePlayerAssignData player)
	{
		this.needRoleAssignPlayer.Add(player);
	}

	public void RemveFromPlayerControl(PlayerControl player)
	{
		this.needRoleAssignPlayer.RemoveAll(
			x =>
				x.PlayerId == player.PlayerId &&
				x.PlayerName == player.Data.DefaultOutfit.PlayerName);
	}

	public void RemvePlayer(VanillaRolePlayerAssignData player)
	{
		this.needRoleAssignPlayer.RemoveAll(x => x == player);
	}

	public void Shuffle()
	{
		this.needRoleAssignPlayer = this.needRoleAssignPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).ToList();
	}
}
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

#nullable enable

public sealed class PlayerRoleAssignData
{
	public IReadOnlyList<NetworkedPlayerInfo> NeedRoleAssignPlayer => this.needRoleAssignPlayer;

	private List<NetworkedPlayerInfo> needRoleAssignPlayer;
	private List<IPlayerToExRoleAssignData> assignData = new List<IPlayerToExRoleAssignData>();
	private Dictionary<byte, ExtremeRoleType> combRoleAssignPlayerId = new Dictionary<byte, ExtremeRoleType>();

	private int gameControlId = 0;

	public PlayerRoleAssignData()
	{
		this.assignData.Clear();

		this.needRoleAssignPlayer = new List<NetworkedPlayerInfo>(
			GameData.Instance.AllPlayers.ToArray());
		this.gameControlId = 0;
	}

	public void AllPlayerAssignToExRole()
	{
		using (var caller = RPCOperator.CreateCaller(
			PlayerControl.LocalPlayer.NetId,
			RPCOperator.Command.SetRoleToAllPlayer))
		{
			caller.WritePackedInt(this.assignData.Count); // 何個あるか

			foreach (IPlayerToExRoleAssignData data in this.assignData)
			{
				caller.WriteByte(data.PlayerId); // PlayerId
				caller.WriteByte(data.RoleType); // RoleType : single or comb
				caller.WritePackedInt(data.RoleId); // RoleId
				caller.WritePackedInt(data.ControlId); // int GameContId

				if (data.RoleType == (byte)IPlayerToExRoleAssignData.ExRoleType.Comb)
				{
					var combData = (PlayerToCombRoleAssignData)data;
					caller.WriteByte(combData.CombTypeId); // combTypeId
					caller.WriteByte(combData.AmongUsRoleId); // byted AmongUsVanillaRoleId
				}
			}
		}
		RPCOperator.SetRoleToAllPlayer(this.assignData);
		RoleAssignState.Instance.SwitchRoleAssignToEnd();
	}

	public IReadOnlyList<NetworkedPlayerInfo> GetCanImpostorAssignPlayer()
	{
		return this.needRoleAssignPlayer.FindAll(
			x =>
			{
				return x.Role.Role is
					RoleTypes.Impostor or
					RoleTypes.Shapeshifter or
					RoleTypes.Phantom;
			});
	}

	public IReadOnlyList<NetworkedPlayerInfo> GetCanCrewmateAssignPlayer()
	{
		return this.needRoleAssignPlayer.FindAll(
			x =>
			{
				return x.Role.Role is
					RoleTypes.Crewmate or
					RoleTypes.Engineer or
					RoleTypes.Scientist or
					RoleTypes.Noisemaker or
					RoleTypes.Tracker;
			});
	}

	public int GetControlId()
	{
		int result = this.gameControlId;

		++this.gameControlId;

		return result;
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

	public void AddPlayer(NetworkedPlayerInfo player)
	{
		this.needRoleAssignPlayer.Add(player);
	}

	public void RemvePlayer(NetworkedPlayerInfo player)
	{
		this.needRoleAssignPlayer.RemoveAll(x => x.PlayerId == player.PlayerId);
	}

	public void Shuffle()
	{
		this.needRoleAssignPlayer = this.needRoleAssignPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).ToList();
	}
}
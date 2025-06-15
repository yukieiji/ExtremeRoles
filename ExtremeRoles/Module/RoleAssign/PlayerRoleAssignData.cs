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

	// 追加メソッド1: RemoveAssignment
	public bool RemoveAssignment(byte playerId, int roleIdToRemove)
	{
		int initialCount = this.assignData.Count;

		this.assignData.RemoveAll(assignment =>
		{
			// IPlayerToExRoleAssignData の実装である PlayerToSingleRoleAssignData と
			// PlayerToCombRoleAssignData が PlayerId と RoleId プロパティを持つことを前提とする。
			// (事前のファイル確認でこれは正しい)
			if (assignment is PlayerToSingleRoleAssignData single)
			{
				return single.PlayerId == playerId && single.RoleId == roleIdToRemove;
			}
			if (assignment is PlayerToCombRoleAssignData comb)
			{
				// コンビ役職の場合、RoleId が個々の役職を指すという前提。
				return comb.PlayerId == playerId && comb.RoleId == roleIdToRemove;
			}
			return false;
		});

		bool removed = this.assignData.Count < initialCount;

		if (removed)
		{
			// combRoleAssignPlayerId から関連データを削除するロジックは、
			// 削除された役職がコンビネーションの一部かどうかの判定が複雑になるため、
			// 今回の変更範囲では含めない。
			// 必要であれば別途対応。
		}
		return removed;
	}

	// 追加メソッド2: AddPlayerToReassign
	public void AddPlayerToReassign(PlayerControl playerControl)
	{
		if (playerControl == null) return;

		if (!this.needRoleAssignPlayer.Any(p => p.PlayerId == playerControl.PlayerId))
		{
			this.needRoleAssignPlayer.Add(new VanillaRolePlayerAssignData(playerControl));
		}
	}
}
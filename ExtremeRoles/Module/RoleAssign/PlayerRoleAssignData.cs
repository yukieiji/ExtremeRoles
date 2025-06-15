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

	public bool RemoveAssignment(byte playerId, int roleIdToRemove)
	{
		int initialCount = this.assignData.Count;
		PlayerToCombRoleAssignData? removedCombAssignmentInfo = null;

		this.assignData.RemoveAll(assignment =>
		{
			bool shouldRemove = false;
			if (assignment is PlayerToSingleRoleAssignData single)
			{
				shouldRemove = single.PlayerId == playerId && single.RoleId == roleIdToRemove;
			}
			else if (assignment is PlayerToCombRoleAssignData comb)
			{
				shouldRemove = comb.PlayerId == playerId && comb.RoleId == roleIdToRemove;
				if (shouldRemove)
				{
					removedCombAssignmentInfo = comb;
				}
			}
			return shouldRemove;
		});

		bool removed = this.assignData.Count < initialCount;

		if (removed && removedCombAssignmentInfo.HasValue)
		{
			PlayerToCombRoleAssignData actualRemovedCombInfo = removedCombAssignmentInfo.Value;

			bool stillHasOtherPartsOfSameCombination = this.assignData.Any(a =>
			{
				if (a is PlayerToCombRoleAssignData otherComb)
				{
					return otherComb.PlayerId == actualRemovedCombInfo.PlayerId &&
						   otherComb.CombTypeId == actualRemovedCombInfo.CombTypeId;
				}
				return false;
			});

			if (!stillHasOtherPartsOfSameCombination)
			{
				// 注意: プレイヤーが複数の異なるタイプのコンビネーション役職を持つケースは稀と想定。
				// もし持つ場合、このロジックでは、あるコンビが完全に消えたら、
				// たとえ別のコンビが残っていても消してしまう可能性がある。
				// より厳密には、combRoleAssignPlayerId の Value (ExtremeRoleType) も考慮するか、
				// CombTypeId ごとに管理する必要があるが、現状の辞書の構造では難しい。
				// ここでは、指定された CombTypeId がなくなった場合に限り、そのプレイヤーの CombRole 情報を消す。
				// playerId に紐づく CombTypeId を管理する構造ではないため、
				// 実際には、その playerId が combRoleAssignPlayerId に登録された際のチーム情報が消えることになる。
				// このキーが CombTypeId ではなく PlayerId であるため、
				// プレイヤーが複数のコンビネーションに同時に属せないという前提に依存する。
				this.combRoleAssignPlayerId.Remove(actualRemovedCombInfo.PlayerId);
			}
		}
		return removed;
	}

	public void AddPlayerToReassign(PlayerControl playerControl)
	{
		if (playerControl == null)
		{
			return;
		}

		if (!this.needRoleAssignPlayer.Any(p => p.PlayerId == playerControl.PlayerId))
		{
			this.needRoleAssignPlayer.Add(new VanillaRolePlayerAssignData(playerControl));
		}
	}
}
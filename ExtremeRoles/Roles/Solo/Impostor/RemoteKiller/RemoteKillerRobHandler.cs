using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerRobHandler(RemoteKillerStatusModel status, RemoteKillerRole role)
{
	private readonly RemoteKillerStatusModel status = status;
	private readonly RemoteKillerRole role = role;
	private PlayerControl? currentRobTarget;
	private PlayerControl? tmpTarget;

	public ReusableActivatingBehavior CreateBehavior(float coolTime, float robActiveTime)
	{
		var behavior = new ReusableActivatingBehavior(
			text: Tr.GetString("remoteKillerRob"),
			img: UnityObjectLoader.LoadSpriteFromResources(ObjectPath.UmbrerFeatVirus),
			canUse: IsUseRob,
			ability: RobStart,
			canActivating: IsRobCheck,
			abilityOff: RobCleanUp,
			forceAbilityOff: RobForceCleanUp);

		behavior.SetCoolTime(coolTime);
		behavior.ActiveTime = robActiveTime;

		return behavior;
	}

	public bool IsUseRob()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		return 
			this.role.IsAbilityUse() && 
			localPlayer != null &&
			Player.TryGetClosestPlayerInRange(localPlayer, this.role, this.status.RobRange, out this.tmpTarget) &&
			!this.status.HasExecutionTarget(this.tmpTarget.PlayerId);
	}

	public bool RobStart()
	{
		this.currentRobTarget = this.tmpTarget;
		return true;
	}

	public bool IsRobCheck()
		=> this.currentRobTarget.IsAlive() &&
			Player.IsPlayerInRangeAndDrawOutLine(
				PlayerControl.LocalPlayer,
				this.currentRobTarget,
				this.role,
				this.status.RobRange);

	public void RobCleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (this.currentRobTarget != null && localPlayer != null)
		{
			this.status.AddExecutionTarget(this.currentRobTarget.PlayerId, localPlayer.PlayerId);
		}

		this.currentRobTarget = null;
	}

	public void RobForceCleanUp()
	{
		this.currentRobTarget = null;
	}

	public void RecordTargetContacts(byte targetId)
	{
		var targetInfo = GameData.Instance.GetPlayerById(targetId);
		if (targetInfo.IsInValid())
		{
			return;
		}

		Vector2 targetPos = targetInfo.Object.GetTruePosition();

		foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (playerInfo.IsInValid() || 
				playerInfo.PlayerId == targetId || 
				playerInfo.Object.inVent)
			{
				continue;
			}

			Vector2 diff = playerInfo.Object.GetTruePosition() - targetPos;
			float dist = diff.magnitude;

			if (dist > this.status.RobRange ||
				PhysicsHelpers.AnyNonTriggersBetween(targetPos, diff.normalized, dist, Constants.ShipAndObjectsMask))
			{
				continue;
			}

			this.status.RecordContact(targetId, playerInfo.PlayerId);
		}
	}
}

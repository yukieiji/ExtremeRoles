using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;

public sealed class RemoteKillerRobHandler(RemoteKillerStatusModel status, RemoteKillerRole role)
{
	private readonly RemoteKillerStatusModel status = status;
	private readonly RemoteKillerRole role = role;
	private PlayerControl? currentRobTarget;

	public ReusableActivatingBehavior CreateBehavior(float coolTime)
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
		behavior.ActiveTime = this.status.RobActiveTime;

		return behavior;
	}

	public bool IsUseRob()
	{
		if (!this.role.IsAbilityUse())
		{
			return false;
		}

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		var target = Player.GetClosestPlayerInRange(localPlayer, this.role, this.status.RobRange);
		if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected)
		{
			return false;
		}

		if (this.status.HasExecutionTarget(target.PlayerId))
		{
			return false;
		}

		if (ExtremeRoleManager.TryGetRole(target.PlayerId, out var targetRole) &&
			this.role.IsSameTeam(targetRole))
		{
			return false;
		}

		return true;
	}

	public bool RobStart()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return false;
		}

		this.currentRobTarget = Player.GetClosestPlayerInRange(localPlayer, this.role, this.status.RobRange);
		return this.currentRobTarget != null;
	}

	public bool IsRobCheck()
	{
		if (this.currentRobTarget == null ||
			this.currentRobTarget.Data == null ||
			this.currentRobTarget.Data.IsDead ||
			this.currentRobTarget.Data.Disconnected)
		{
			return false;
		}

		return Player.IsPlayerInRangeAndDrawOutLine(
			PlayerControl.LocalPlayer,
			this.currentRobTarget,
			this.role,
			this.status.RobRange);
	}

	public void RobCleanUp()
	{
		if (this.currentRobTarget != null)
		{
			byte targetId = this.currentRobTarget.PlayerId;
			this.status.AddExecutionTarget(targetId);
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
		if (targetInfo == null || targetInfo.IsDead || targetInfo.Disconnected || !targetInfo.Object)
		{
			return;
		}

		Vector2 targetPos = targetInfo.Object.GetTruePosition();

		foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (playerInfo.IsInValid() || playerInfo.PlayerId == targetId || !playerInfo.Object || playerInfo.Object.inVent)
			{
				continue;
			}

			Vector2 diff = playerInfo.Object.GetTruePosition() - targetPos;
			float dist = diff.magnitude;

			if (dist > this.status.RobRange)
			{
				continue;
			}

			if (PhysicsHelpers.AnyNonTriggersBetween(targetPos, diff.normalized, dist, Constants.ShipAndObjectsMask))
			{
				continue;
			}

			this.status.RecordContact(targetId, playerInfo.PlayerId);
		}
	}
}

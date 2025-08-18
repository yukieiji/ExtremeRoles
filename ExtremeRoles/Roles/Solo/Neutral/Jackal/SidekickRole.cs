using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Module.CustomOption.Factory.Old;


namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

#nullable enable

public sealed class SidekickRole : SingleRoleBase, IRoleUpdate
{
	public byte Parent { get; }
	private readonly int recursion = 0;
	private readonly bool sidekickJackalCanMakeSidekick = false;

	public override IStatusModel? Status => status;
	public override IOptionLoader Loader { get; }

	private readonly SidekickStatus status;

	public SidekickRole(
		JackalRole jackal,
		byte jackalPlayerId,
		bool isImpostor,
		JackalRole.SidekickOptionHolder option) : base(
			RoleCore.BuildNeutral(
				ExtremeRoleId.Sidekick,
				ColorPalette.JackalBlue),
			option.CanKill, false,
			option.UseVent, option.UseSabotage)
	{

		this.Loader = jackal.Loader;
		this.status = new SidekickStatus(jackalPlayerId, jackal);

		this.Parent = jackalPlayerId;
		SetControlId(jackal.GameControlId);

		HasOtherKillCool = option.HasOtherKillCool;
		KillCoolTime = option.KillCool;
		HasOtherKillRange = option.HasOtherKillRange;
		KillRange = option.KillRange;

		HasOtherVision = option.HasOtherVision;
		Vision = option.Vision;
		IsApplyEnvironmentVision = option.ApplyEnvironmentVisionEffect;

		FakeImpostor = jackal.CanSeeImpostorToSidekickImpostor && isImpostor;

		recursion = jackal.CurRecursion;
		sidekickJackalCanMakeSidekick = jackal.SidekickJackalCanMakeSidekick;
	}

	public override bool IsSameTeam(SingleRoleBase targetRole)
	{
		if (isSameJackalTeam(targetRole))
		{
			if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
			{
				return true;
			}
			else
			{
				return IsSameControlId(targetRole);
			}
		}
		else
		{
			return base.IsSameTeam(targetRole);
		}
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{
		if (targetRole.Core.Id is ExtremeRoleId.Jackal &&
			targetPlayerId == this.Parent)
		{
			return ColorPalette.JackalBlue;
		}
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public override string GetFullDescription()
	{
		var jackal = Player.GetPlayerControlById(this.Parent);
		string fullDesc = base.GetFullDescription();

		if (jackal == null ||
			jackal.Data == null)
		{
			return fullDesc;
		}

		return string.Format(
			fullDesc, jackal.Data.PlayerName);
	}

	public static void BecomeToJackal(byte callerId, byte targetId)
	{

		var curJackal = ExtremeRoleManager.GetSafeCastedRole<JackalRole>(callerId);
		if (curJackal == null) { return; }
		var newJackal = (JackalRole)curJackal.Clone();

		bool multiAssignTrigger = false;
		var curRole = ExtremeRoleManager.GameRole[targetId];
		var curSideKick = curRole as SidekickRole;
		if (curSideKick == null)
		{
			curSideKick = (SidekickRole)((MultiAssignRoleBase)curRole).AnotherRole!;
			multiAssignTrigger = true;
		}

		newJackal.Initialize();
		if (targetId == PlayerControl.LocalPlayer.PlayerId)
		{
			newJackal.CreateAbility();
		}

		if ((
				!curSideKick.sidekickJackalCanMakeSidekick ||
				curSideKick.recursion >= newJackal.SidekickRecursionLimit
			) &&
			newJackal.Button?.Behavior is ICountBehavior countBehavior)
		{
			countBehavior.SetAbilityCount(0);
		}

		newJackal.CurRecursion = curSideKick.recursion + 1;
		newJackal.SidekickPlayerId = [.. curJackal.SidekickPlayerId];
		newJackal.SetControlId(curSideKick.GameControlId);

		newJackal.SidekickPlayerId.Remove(targetId);

		if (multiAssignTrigger)
		{
			var multiAssignRole = (MultiAssignRoleBase)curRole;
			multiAssignRole.AnotherRole = null;
			multiAssignRole.CanKill = false;
			multiAssignRole.HasTask = false;
			multiAssignRole.UseSabotage = false;
			multiAssignRole.UseVent = false;

			ExtremeRoleManager.SetNewAnothorRole(targetId, newJackal);
		}
		else
		{
			ExtremeRoleManager.SetNewRole(targetId, newJackal);
		}

	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (Player.GetPlayerControlById(this.Parent).Data.Disconnected)
		{
			this.status.ClearSidekick();
			ExtremeRoleManager.RpcReplaceRole(
				this.Parent, rolePlayer.PlayerId,
				ExtremeRoleManager.ReplaceOperation.SidekickToJackal);
		}
	}

	private bool isSameJackalTeam(SingleRoleBase targetRole)
	{
		var id = targetRole.Core.Id;
		return id == Core.Id || id is ExtremeRoleId.Jackal;
	}
}

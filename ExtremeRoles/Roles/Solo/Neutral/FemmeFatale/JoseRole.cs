using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

public sealed class JoseRole :
	MultiAssignRoleBase,
	IRoleMurderPlayerHook,
	IRoleUpdate,
	IRoleResetMeeting
{
	public const string RoleShowTag = "◯j";

	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly JoseStatus status;
	private readonly JoseStatusModel statusModel;

	public JoseRole(
		byte femmeFatalePlayerId,
		FemmeFataleRole femmeFatale,
		SingleRoleBase baseRole,
		float selfKillTimerTime) :
		base(
			new RoleArgs(
				new RoleCore(ExtremeRoleId.Jose, baseRole.Core.Team, ColorPalette.FemmeFataleCrimson),
				createJoseRoleProp(baseRole)))
	{
		this.Loader = femmeFatale.Loader;
		this.status = new JoseStatus(femmeFatalePlayerId, femmeFatale);
		this.statusModel = new JoseStatusModel(selfKillTimerTime);
		SetControlId(femmeFatale.GameControlId);

		CanKill = baseRole.CanKill;

		if (baseRole.IsImpostor())
		{
			HasOtherVision = true;
		}
		else
		{
			HasOtherVision = baseRole.HasOtherVision;
		}
		Vision = baseRole.Vision;

		IsApplyEnvironmentVision = baseRole.IsApplyEnvironmentVision;

		HasOtherKillCool = baseRole.HasOtherKillCool;
		KillCoolTime = baseRole.KillCoolTime;
		HasOtherKillRange = baseRole.HasOtherKillRange;
		KillRange = baseRole.KillRange;
	}

	public void HookMurderPlayer(
		PlayerControl source, PlayerControl target)
	{
		if (source.PlayerId != target.PlayerId)
		{
			this.statusModel.OnKill();
		}
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel.OnMeetingEnd();
	}

	public void ResetOnMeetingStart()
	{
		this.statusModel.OnMeetingStart();
	}

	public override string GetRoleTag() => RoleShowTag;

	public void Update(PlayerControl rolePlayer)
	{
		this.statusModel.Update(rolePlayer, Time.deltaTime);
	}

	public static void RemoveJose(byte targetPlayerId)
	{
		if (!ExtremeRoleManager.TryGetRole(targetPlayerId, out var currentRole))
		{
			return;
		}

		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			if (currentRole is JoseRole jr)
			{
				jr.statusModel.Cleanup();
			}
			else if (currentRole is MultiAssignRoleBase multiJr && multiJr.AnotherRole is JoseRole anotherJr)
			{
				anotherJr.statusModel.Cleanup();
			}
		}

		if (currentRole is JoseRole joseRole)
		{
			if (joseRole.AnotherRole != null)
			{
				ExtremeRoleManager.SetNewRole(targetPlayerId, joseRole.AnotherRole);
			}
		}
		else if (currentRole is MultiAssignRoleBase multiRole && multiRole.AnotherRole is JoseRole)
		{
			multiRole.AnotherRole = null;
			multiRole.CanHasAnotherRole = true;
			ExtremeRoleManager.SetNewRole(targetPlayerId, multiRole);
		}
	}

	private static RoleProp createJoseRoleProp(SingleRoleBase role)
	{
		var prop = RolePropPresets.OptionalDefault;
		if (role.CanKill)
		{
			prop |= RoleProp.CanKill;
		}
		if (!role.IsImpostor() || role.HasTask)
		{
			prop |= RoleProp.HasTask;
		}
		if (role.UseVent)
		{
			prop |= RoleProp.UseVent;
		}
		if (role.UseSabotage)
		{
			prop |= RoleProp.UseSabotage;
		}
		return prop;
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		throw new Exception("Don't call this class method!!");
	}

	protected override void RoleSpecificInit()
	{
		throw new Exception("Don't call this class method!!");
	}
}

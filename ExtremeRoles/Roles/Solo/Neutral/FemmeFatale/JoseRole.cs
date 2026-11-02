using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;
using ExtremeRoles.Roles.Solo.Crewmate;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

public sealed class JoseRole :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleMurderPlayerHook,
	IRoleUpdate,
	IRoleResetMeeting,
	ITryKillTo
{
	public const string RoleShowTag = "◯j";

	public ExtremeAbilityButton? Button { get; set; }

	private readonly byte femmeFatalePlayerId;
	public override IOptionLoader Loader { get; }
	public override IStatusModel Status => status;
	private readonly JoseStatus status;
	private readonly JoseStatusModel statusModel;
	private TextMeshPro? timerText;
	private HashSet<uint> completedTasks = [];

	public bool IsSpecialKill { get; }
	public SingleRoleBase OriginalRole { get; }

	public JoseRole(
		byte femmeFatalePlayerId,
		FemmeFataleRole femmeFatale,
		SingleRoleBase baseRole,
		float selfKillTimerTime) :
		base(
			RoleArgs.BuildNeutral(
				ExtremeRoleId.Jose,
				ColorPalette.FemmeFataleCrimson,
				createJoseRoleProp(baseRole)))
	{
		this.Loader = femmeFatale.Loader;
		this.femmeFatalePlayerId = femmeFatalePlayerId;
		this.OriginalRole = baseRole;
		this.status = new JoseStatus(femmeFatalePlayerId, femmeFatale, baseRole);
		this.statusModel = new JoseStatusModel(selfKillTimerTime);
		SetControlId(femmeFatale.GameControlId);

		var core = baseRole.Core;
		var id = core.Id;

		IsSpecialKill = id is ExtremeRoleId.Fencer or ExtremeRoleId.Sheriff;

		CanKill = id switch
		{
			ExtremeRoleId.Fencer => false,
			ExtremeRoleId.Sheriff or ExtremeRoleId.Yandere or ExtremeRoleId.Hero => true,
			_ => CanKill,
		};

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

	public void SelfKillAbility(float coolTime)
	{
		Button = RoleAbilityFactory.CreateReusableAbility(
			"selfKill",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.SucideSprite),
			IsAbilityUse,
			UseAbility);
		Button.Behavior.SetCoolTime(coolTime);
		Button.OnMeetingEnd();
	}

	public void HookMurderPlayer(
		PlayerControl source, PlayerControl target)
	{
		if (source.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
			source.PlayerId != target.PlayerId)
		{
			this.statusModel.Reset();
		}
	}

	public void CreateAbility()
	{
		throw new Exception("Don't call this class method!!");
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel.Reset();
		this.completedTasks.Clear();
	}

	public void ResetOnMeetingStart()
	{
		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(false);
		}
		this.statusModel.Reset();
	}

	public bool UseAbility()
	{
		byte playerId = PlayerControl.LocalPlayer.PlayerId;
		Player.RpcUncheckMurderPlayer(
			playerId, playerId, byte.MaxValue);

		return true;
	}

	public override void OverrideAnotherRoleSetting()
	{
		var femmePlayer = GameData.Instance.GetPlayerById(this.status.Parent);

		if (AnotherRole is Resurrecter resurrecter &&
			femmePlayer.IsInValid())
		{
			Resurrecter.UseResurrect(resurrecter);
		}
	}

	public bool TryRolePlayerKillTo(
		PlayerControl rolePlayer, PlayerControl targetPlayer)
	{
		if (targetPlayer.PlayerId != femmeFatalePlayerId)
		{
			return true;
		}

		if (AnotherRole?.Core.Id is ExtremeRoleId.Sheriff)
		{
			Player.RpcUncheckMurderPlayer(
				rolePlayer.PlayerId,
				rolePlayer.PlayerId,
				byte.MaxValue);

			ExtremeRolesPlugin.ShipState.RpcReplaceDeadReason(
				rolePlayer.PlayerId, ExtremeShipStatus.PlayerStatus.MissShot);
		}
		return false;
	}

	public override string GetRoleTag() => RoleShowTag;

	public override string GetRolePlayerNameTag(
		SingleRoleBase targetRole, byte targetPlayerId)
	{
		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			return Design.ColoredString(
				ColorPalette.FemmeFataleCrimson,
				$" {RoleShowTag}");
		}

		return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
	}

	public override Color GetTargetRoleSeeColor(
		SingleRoleBase targetRole,
		byte targetPlayerId)
	{
		return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (!GameProgressSystem.IsTaskPhase || rolePlayer.IsInValid())
		{
			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(false);
			}
			return;
		}

		// Full task completion resets self-destruct timer
		if (rolePlayer.Data != null && rolePlayer.Data.Tasks != null)
		{
			foreach (var task in rolePlayer.Data.Tasks)
			{
				if (task.Complete && !this.completedTasks.Contains(task.Id))
				{
					this.completedTasks.Add(task.Id);
					this.statusModel.Reset();
				}
			}
		}

		if (rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			bool explode = this.statusModel.UpdateTimer(Time.deltaTime);
			if (explode)
			{
				Player.RpcUncheckMurderPlayer(
					rolePlayer.PlayerId,
					rolePlayer.PlayerId,
					byte.MaxValue);
			}

			if (this.timerText == null &&
				HudManager.Instance != null &&
				HudManager.Instance.KillButton != null &&
				HudManager.Instance.KillButton.cooldownTimerText != null &&
				HudManager.Instance.UseButton != null)
			{
				createTimerText(HudManager.Instance);
			}

			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(true);
				this.timerText.text = $"{Mathf.CeilToInt(this.statusModel.CurrentSelfKillTimer)}";
			}
		}
	}

	public static void RemoveJose(byte targetPlayerId)
	{
		if (!ExtremeRoleManager.TryGetRole(targetPlayerId, out var currentRole))
		{
			return;
		}

		SingleRoleBase? originalRole = null;
		if (currentRole is JoseRole joseRole)
		{
			originalRole = joseRole.OriginalRole;
		}
		else if (currentRole is MultiAssignRoleBase multi && multi.AnotherRole is JoseRole anotherJose)
		{
			originalRole = anotherJose.OriginalRole;
		}

		if (originalRole == null)
		{
			return;
		}

		if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			if (currentRole is JoseRole jr && jr.timerText != null)
			{
				jr.timerText.gameObject.SetActive(false);
				UnityEngine.Object.Destroy(jr.timerText.gameObject);
			}
		}

		if (currentRole is MultiAssignRoleBase multiRole && multiRole.AnotherRole is JoseRole)
		{
			multiRole.AnotherRole = null;
			multiRole.CanHasAnotherRole = true;
			ExtremeRoleManager.SetNewRole(targetPlayerId, multiRole);
		}
		else
		{
			ExtremeRoleManager.SetNewRole(targetPlayerId, originalRole);
		}
	}

	private void createTimerText(HudManager hud)
	{
		var killButton = hud.KillButton;

		this.timerText = UnityEngine.Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.transform.parent);
		this.timerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		this.timerText.transform.localPosition =
			hud.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
		this.timerText.gameObject.SetActive(true);

		var timerInfoText = UnityEngine.Object.Instantiate(
			hud.KillButton.cooldownTimerText,
			this.timerText.transform);
		timerInfoText.enableWordWrapping = false;
		timerInfoText.transform.localScale = Vector3.one * 0.5f;
		timerInfoText.transform.localPosition += new Vector3(-0.05f, 0.6f, 0);
		timerInfoText.gameObject.SetActive(true);

		timerInfoText.text = Tr.GetString("untilSelfKill");
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

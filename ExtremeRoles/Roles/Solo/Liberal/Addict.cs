using System;
using UnityEngine;
using TMPro;
using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class Addict :
	SingleRoleBase,
	IRoleUpdate,
	IRoleResetMeeting
{
	public enum AddictOption
	{
		SelfKillTimerTime,
		MovementTimeToRecover,
	}

	private DoveCommonAbilityHandler? handler;
	private AddictStatusModel? statusModel;
	private TextMeshPro? timerText;

	public override IStatusModel? Status => statusModel;

	public Addict() : base(
		RoleArgs.BuildLiberalDove(ExtremeRoleId.Addict))
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.handler?.Update(rolePlayer);

		if (this.statusModel is null)
		{
			return;
		}

		if (!GameProgressSystem.IsTaskPhase || rolePlayer.IsInValid())
		{
			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(false);
			}
			return;
		}

		bool explode = this.statusModel.UpdateTimer(rolePlayer, Time.deltaTime);
		if (explode)
		{
			var localPlayer = PlayerControl.LocalPlayer;
			if (localPlayer != null && rolePlayer.PlayerId == localPlayer.PlayerId)
			{
				Player.RpcUncheckMurderPlayer(
					rolePlayer.PlayerId,
					rolePlayer.PlayerId,
					byte.MaxValue);
			}
		}

		var currentLocalPlayer = PlayerControl.LocalPlayer;
		if (currentLocalPlayer != null && rolePlayer.PlayerId == currentLocalPlayer.PlayerId)
		{
			if (this.timerText == null &&
				HudManager.Instance != null &&
				HudManager.Instance.KillButton != null &&
				HudManager.Instance.KillButton.cooldownTimerText != null &&
				HudManager.Instance.UseButton != null)
			{
				createTimerText();
			}

			if (this.timerText != null)
			{
				this.timerText.gameObject.SetActive(true);
				this.timerText.text = Tr.GetString("addictSelfKill", Mathf.CeilToInt(this.statusModel.CurrentSelfKillTimer));
			}
		}
	}

	public void ResetOnMeetingStart()
	{
		if (this.timerText != null)
		{
			this.timerText.gameObject.SetActive(false);
		}
		this.statusModel?.Reset();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.statusModel?.Reset();
	}

	public override void ExiledAction(PlayerControl rolePlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		this.handler?.ClearTask(rolePlayer);
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateFloatOption(
			AddictOption.SelfKillTimerTime,
			15.0f, 5.0f, 120.0f, 1.0f,
			format: OptionUnit.Second);

		factory.CreateFloatOption(
			AddictOption.MovementTimeToRecover,
			5.0f, 1.0f, 15.0f, 0.5f,
			format: OptionUnit.Second);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;

		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
		this.handler = new DoveCommonAbilityHandler(liberalOption);

		float maxTimer = loader.GetValue<AddictOption, float>(AddictOption.SelfKillTimerTime);
		float recoveryTime = loader.GetValue<AddictOption, float>(AddictOption.MovementTimeToRecover);

		this.statusModel = new AddictStatusModel(maxTimer, recoveryTime);
	}

	private void createTimerText()
	{
		if (HudManager.Instance == null ||
			HudManager.Instance.KillButton == null ||
			HudManager.Instance.KillButton.cooldownTimerText == null ||
			HudManager.Instance.UseButton == null) { return; }

		var hudManager = HudManager.Instance;
		var killButton = hudManager.KillButton;

		this.timerText = UnityEngine.Object.Instantiate(
			killButton.cooldownTimerText,
			killButton.transform.parent);
		this.timerText.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
		this.timerText.transform.localPosition =
			hudManager.UseButton.transform.localPosition + new Vector3(-2.0f, -0.125f, 0);
		this.timerText.gameObject.SetActive(true);
	}
}

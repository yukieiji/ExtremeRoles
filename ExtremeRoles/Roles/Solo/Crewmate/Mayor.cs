using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Mayor :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleResetMeeting
{
	public enum MayorOption
	{
		TargetRange,
	}

	public ExtremeAbilityButton? Button { get; set; }

	private PlayerControl? tmpPlayer;
	private PlayerControl? targetPlayer;
	private float targetRange;
	private static TextPopUpper? popUpper;

	public Mayor() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Mayor,
			ColorPalette.MayorOchre))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"mayorReveal",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Mayor),
			checkAbility: CheckAbility,
			abilityOff: CleanUp,
			forceAbilityOff: ForceAbilityOff);
		this.Button?.SetLabelToCrewmate();
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() &&
			Player.TryGetClosestPlayerInRange(this, this.targetRange, out this.tmpPlayer);

	public bool UseAbility()
	{
		if (this.tmpPlayer == null)
		{
			return false;
		}

		this.targetPlayer = this.tmpPlayer;
		return true;
	}

	public bool CheckAbility()
		=> this.targetPlayer != null &&
			Player.IsPlayerInRangeAndDrawOutLine(
				PlayerControl.LocalPlayer,
				this.targetPlayer,
				this,
				this.targetRange);

	public void ForceAbilityOff()
	{
		this.targetPlayer = null;
	}

	public void CleanUp()
	{
		if (this.targetPlayer != null)
		{
			byte mayorId = PlayerControl.LocalPlayer.PlayerId;
			byte targetId = this.targetPlayer.PlayerId;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MayorRevealAbility))
			{
				caller.WriteByte(mayorId);
				caller.WriteByte(targetId);
			}

			Reveal(mayorId, targetId);
		}
		this.targetPlayer = null;
	}

	public static void Reveal(byte mayorId, byte targetId)
	{
		if (!Player.TryGetPlayerControl(mayorId, out var mayorPlayer) ||
			!Player.TryGetPlayerControl(targetId, out var targetPlayer))
		{
			return;
		}

		if (PlayerControl.LocalPlayer == null)
		{
			return;
		}

		byte localId = PlayerControl.LocalPlayer.PlayerId;
		if (localId == mayorId)
		{
			string message = string.Format(Tr.GetString("mayorRevealToSelf"), targetPlayer.Data.PlayerName);
			ShowPopUp(message);
			Sound.PlaySound(Sound.Type.MayorReveal, 0.8f);
		}
		else if (localId == targetId)
		{
			string message = string.Format(Tr.GetString("mayorRevealToTarget"), mayorPlayer.Data.PlayerName);
			ShowPopUp(message);
			Sound.PlaySound(Sound.Type.MayorReveal, 0.8f);
		}
	}

	public static void ShowPopUp(string message)
	{
		popUpper ??= new TextPopUpper(
			5, 5.0f,
			new Vector3(-3.75f, -3.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		popUpper.AddText(message);
	}

	public void ResetOnMeetingStart()
	{
		popUpper?.Clear();
		popUpper = null;
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.tmpPlayer = null;
		this.targetPlayer = null;
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(
			factory,
			defaultAbilityCount: 1,
			maxAbilityCount: 10,
			defaultActiveTime: 3.0f);

		factory.CreateFloatOption(
			MayorOption.TargetRange,
			1.5f, 0.5f, 5.0f, 0.25f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.targetRange = loader.GetValue<MayorOption, float>(MayorOption.TargetRange);
	}
}

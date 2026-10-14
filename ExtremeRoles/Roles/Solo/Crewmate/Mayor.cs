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
			Player.TryGetClosestPlayerInRange(this, this.targetRange, out this.targetPlayer);

	public bool UseAbility()
	{
		if (this.targetPlayer == null)
		{
			return false;
		}

		byte mayorId = PlayerControl.LocalPlayer.PlayerId;
		byte targetId = this.targetPlayer.PlayerId;

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.MayorRevealAbility))
		{
			caller.WriteByte(mayorId);
			caller.WriteByte(targetId);
		}

		Reveal(mayorId, targetId);
		return true;
	}

	public bool CheckAbility()
		=> IRoleAbility.IsCommonUse();

	public void ForceAbilityOff()
	{
		ClearPopUp();
	}

	public void CleanUp()
	{
		ClearPopUp();
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
			ShowPopUp(message, 60.0f);
			Sound.PlaySound(Sound.Type.MayorReveal, 0.8f);
		}
		else if (localId == targetId)
		{
			string message = string.Format(Tr.GetString("mayorRevealToTarget"), mayorPlayer.Data.PlayerName);
			ShowPopUp(message, 3.0f);
			Sound.PlaySound(Sound.Type.MayorReveal, 0.8f);
		}
	}

	public static void ShowPopUp(string message, float disappearTime = 60.0f)
	{
		ClearPopUp();
		popUpper = new TextPopUpper(
			1, disappearTime,
			new Vector3(-3.75f, -3.5f, -250.0f),
			TMPro.TextAlignmentOptions.BottomLeft);
		popUpper.AddText(message);
	}

	public static void ClearPopUp()
	{
		popUpper?.Clear();
		popUpper = null;
	}

	public void ResetOnMeetingStart()
	{
		ClearPopUp();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.targetPlayer = null;
		ClearPopUp();
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

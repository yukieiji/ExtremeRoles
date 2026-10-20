using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Player;
using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Mayor :
	SingleRoleBase,
	IRoleAutoBuildAbility
{
	public enum MayorOption
	{
		TargetRange,
	}

	public ExtremeAbilityButton? Button { get; set; }

	public readonly record struct TextKey(byte MayorPlayerId, byte TargetPlayerId);

	private Dictionary<TextKey, TextPopUpper>? popUpper;

	private PlayerControl? targetPlayer;
	private float targetRange;

	public Mayor() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Mayor,
			ColorPalette.MayorOchre))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"mayorReveal",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Mayor));
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

	public static void Reveal(byte mayorId, byte targetId)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!ExtremeRoleManager.TryGetSafeCastedRole<Mayor>(mayorId, out var role) ||
			!Player.TryGetPlayerControl(mayorId, out var mayorPlayer) ||
			!Player.TryGetPlayerControl(targetId, out var targetPlayer) ||
			localPlayer.IsInValid() ||
			targetPlayer.IsInValid())
		{
			return;
		}

		byte localId = localPlayer.PlayerId;

		var key = new TextKey(mayorId, targetId);

		if (localId == mayorId)
		{
			role.ShowPopUp(key, Tr.GetString("mayorRevealToSelf", targetPlayer.Data.DefaultOutfit.PlayerName), 5.0f);
		}
		else if (localId == targetId)
		{
			role.ShowPopUp(key, Tr.GetString("mayorRevealToTarget", mayorPlayer.Data.DefaultOutfit.PlayerName), 3.0f);
		}
	}

	public void ShowPopUp(in TextKey key, string message, float disappearTime = 60.0f)
	{
		if (this.popUpper is null)
		{
			this.popUpper = [];
		}

		if (!this.popUpper.TryGetValue(key, out var text))
		{
			text = new TextPopUpper(
				1, disappearTime,
				new Vector3(0.0f, 1.25f, -250.0f),
				TMPro.TextAlignmentOptions.Center);
			this.popUpper[key] = text;
		}
		
		text.AddText(message);
	}

	public void ClearPopUp()
	{
		if (this.popUpper is null)
		{
			return;
		}

		foreach (var text in this.popUpper.Values)
		{
			text.Clear();
		}
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
			maxAbilityCount: 10);

		factory.CreateFloatOption(
			MayorOption.TargetRange,
			1.5f, 0.5f, 5.0f, 0.25f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.targetRange = loader.GetValue<MayorOption, float>(MayorOption.TargetRange);
		this.popUpper = [];
	}
}

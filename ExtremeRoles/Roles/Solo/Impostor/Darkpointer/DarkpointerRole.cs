using Hazel;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Darkpointer;

public sealed class DarkpointerRole :
	SingleRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialReset
{
	public enum Option
	{
		ActiveDuration,
		Range,
	}

	public ExtremeAbilityButton? Button { get; set; }
	private DarkpointerDarknessSystem? system;

	public DarkpointerRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Darkpointer))
	{ }

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			Tr.GetString("DarkpointerAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Darkpointer),
			IsAbilityCheck,
			CleanUp,
			ForceCleanUp,
			isReduceOnActive: true);
	}

	public bool IsAbilityCheck() => true;

	public bool UseAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!localPlayer.IsInValid())
		{
			Vector2 pos = localPlayer.GetTruePosition();
			this.system?.RpcPlaceDarkness(localPlayer.PlayerId, pos);
		}
		return true;
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void CleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!localPlayer.IsInValid())
		{
			this.system?.RpcRemoveDarkness(localPlayer.PlayerId);
		}
	}

	public void ForceCleanUp()
	{
		CleanUp();
	}

	public void ResetOnMeetingStart() { }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null) { }

	public void AllReset(PlayerControl rolePlayer) { }

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10, 10.0f);
		factory.CreateFloatOption(Option.Range, 2.5f, 1.0f, 5.0f, 0.25f);
	}

	protected override void RoleSpecificInit()
	{
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.DarkpointerDarkness,
			() => new DarkpointerDarknessSystem(this.Loader.GetValue<Option, float>(Option.Range)));
	}
}

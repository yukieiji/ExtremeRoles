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

	private float activeDuration;
	private float range;
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
		if (!localPlayer.IsInValid() && this.system != null)
		{
			Vector2 pos = localPlayer.GetTruePosition();
			this.system.RpcPlaceDarkness(pos, this.range, this.activeDuration);
		}
		return true;
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void CleanUp() { }

	public void ForceCleanUp() { }

	public void ResetOnMeetingStart() { }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null) { }

	public void AllReset(PlayerControl rolePlayer) { }

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10);
		factory.CreateFloatOption(Option.ActiveDuration, 15f, 5f, 60f, 1f, format: OptionUnit.Second);
		factory.CreateFloatOption(Option.Range, 2.5f, 1.0f, 5.0f, 0.25f);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.activeDuration = loader.GetValue<Option, float>(Option.ActiveDuration);
		this.range = loader.GetValue<Option, float>(Option.Range);

		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet<DarkpointerDarknessSystem>(
			ExtremeSystemType.DarkpointerDarkness);
	}
}

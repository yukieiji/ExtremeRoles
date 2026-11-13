using ExtremeRoles.Extension.Player;
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
		Range,
	}

	public ExtremeAbilityButton? Button { get; set; }
	private DarkpointerDarknessSystem? system;

	public DarkpointerRole() : base(
		RoleArgs.BuildImpostor(ExtremeRoleId.Darkpointer))
	{ }

	public void CreateAbility()
	{
		this.CreateReclickableCountAbilityButton(
			Tr.GetString("DarkpointerAbility"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Darkpointer),
			IsAbilityCheck,
			CleanUp);
	}

	public bool IsAbilityCheck() => true;

	public bool UseAbility()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsAlive())
		{
			this.system?.RpcPlaceDarkness(localPlayer.PlayerId);
		}
		return true;
	}

	public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public void CleanUp()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer.IsAlive())
		{
			this.system?.RpcRemoveDarkness(localPlayer.PlayerId);
		}
	}

	public void ResetOnMeetingStart() { }

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null) { }

	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		reset(rolePlayer);
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		reset(rolePlayer);
	}

	private void reset(PlayerControl rolePlayer)
	{
		if (rolePlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId)
		{
			CleanUp();
		}
	}

	protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 1, 10, 10.0f);
		factory.CreateFloatOption(Option.Range, 10.0f, 1.0f, 50.0f, 0.5f);
	}

	protected override void RoleSpecificInit()
	{
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.DarkpointerDarkness,
			() => new DarkpointerDarknessSystem(this.Loader.GetValue<Option, float>(Option.Range)));
	}
}

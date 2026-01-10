using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Inspector : SingleRoleBase, IRoleAutoBuildAbility
{
	public ExtremeAbilityButton? Button { get; set; }

	public enum Option
    {
		InspectSabotage,
		InspectVent,
		InspectAbility,
    }

    public Inspector() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Inspector,
			ColorPalette.InspectorAmberYellow))
    { }

	/// <summary>
	/// インスペクターがキルされた時に、インスペクターのシステムから役職プレイヤー自体を削除する
	/// </summary>
	/// <param name="rolePlayer">役職プレイヤー</param>
	/// <param name="killerPlayer">キラー</param>
	public override void RolePlayerKilledAction(PlayerControl rolePlayer, PlayerControl killerPlayer)
	{
		if (ExtremeSystemTypeManager.Instance.TryGet<InspectorInspectSystem>(ExtremeSystemType.InspectorInspect, out var system))
		{
			system.EndInspect(rolePlayer.PlayerId);
		}
	}

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		IRoleAbility.CreateAbilityCountOption(factory, 3, 10, 5);

		factory.CreateBoolOption(Option.InspectSabotage, true);
		factory.CreateBoolOption(Option.InspectVent, true);
		factory.CreateBoolOption(Option.InspectAbility, false);
	}

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;

		var mode = InspectorInspectSystem.InspectMode.None;
		if (loader.GetValue<Option, bool>(Option.InspectSabotage))
		{
			mode |= InspectorInspectSystem.InspectMode.Sabotage;
		}
		if (loader.GetValue<Option, bool>(Option.InspectVent))
		{
			mode |= InspectorInspectSystem.InspectMode.Vent;
		}
		if (loader.GetValue<Option, bool>(Option.InspectAbility))
		{
			mode |= InspectorInspectSystem.InspectMode.Ability;
		}

		ExtremeSystemTypeManager.Instance.TryAdd(ExtremeSystemType.InspectorInspect, new InspectorInspectSystem(mode));
	}

	public bool UseAbility()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.InspectorInspect, x => x.Write((byte)InspectorInspectSystem.Ops.StartInspect));
		return true;
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse();

	public void CreateAbility()
	{
		this.CreateActivatingAbilityCountButton(
			"inspect",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Inspector),
			abilityOff: CleanUp,
			forceAbilityOff: CleanUp);
		this.Button?.SetLabelToCrewmate();
	}

	public void CleanUp()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.InspectorInspect, x => x.Write((byte)InspectorInspectSystem.Ops.EndInspect));
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{

	}

	public void ResetOnMeetingStart()
	{
		CleanUp();
	}
}

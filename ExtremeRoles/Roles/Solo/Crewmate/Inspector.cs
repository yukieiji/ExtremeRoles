using ExtremeRoles.Module;

using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Inspector : SingleRoleBase, IRoleAutoBuildAbility
{
	public ExtremeAbilityButton Button { get; set; }

	public enum Option
    {
    }

    public Inspector() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Inspector,
			ColorPalette.BakaryWheatColor),
        false, true, false, false)
    { }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		IRoleAbility.CreateAbilityCountOption(factory, 3, 10, 5);
    }

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;

		ExtremeSystemTypeManager.Instance.TryAdd(
			ExtremeSystemType.InspectorInspect,
			new InspectorInspectSystem(InspectorInspectSystem.InspectMode.Ability));
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
			"curse",
			UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.CurseMakerCurse),
			abilityOff: CleanUp,
			forceAbilityOff: CleanUp);
		this.Button.SetLabelToCrewmate();
	}

	public void CleanUp()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.InspectorInspect, x => x.Write((byte)InspectorInspectSystem.Ops.EndInspect));
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
	{

	}

	public void ResetOnMeetingStart()
	{
		CleanUp();
	}
}

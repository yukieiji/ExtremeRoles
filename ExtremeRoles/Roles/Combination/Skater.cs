using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class SkaterManager : FlexibleCombinationRoleManagerBase
{
    public SkaterManager() : base(
		CombinationRoleType.Skater, new Skater(), 1)
    { }

}

public sealed class SkaterStatus(float canUseSpeed) : IStatusModel, IUsableOverrideStatus
{
	public bool EnableUseButton
		=> this.Behaviour == null || !this.Behaviour.enabled || this.Behaviour.PrevForce.magnitude <= this.canUseSpeed;

	public bool EnableVentButton
		=> this.Behaviour == null || !this.Behaviour.enabled || this.Behaviour.PrevForce.magnitude <= this.canUseSpeed;

	private readonly float canUseSpeed = canUseSpeed;

	public SkaterSkateBehaviour? Behaviour { get; set; }
}

#nullable enable

public sealed class Skater :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialSetUp,
	IRoleSpecialReset
{
	public enum Option
	{
		Acceleration,
		MaxSpeed,
		Friction,
		UseE,
		EValue,
		CanUseSpeed
	}

    public override string RoleName =>
        string.Concat(this.roleNamePrefix, this.Core.Name);


	public ExtremeAbilityButton? Button { get; set; }

	public override IStatusModel? Status => status;
	private SkaterStatus? status;

    private string roleNamePrefix = string.Empty;

    public Skater(
        ) : base(
			RoleCore.BuildCrewmate(ExtremeRoleId.Skater, ColorPalette.SkaterMizuiro),
            false, true, false, false,
            tab: OptionTab.CombinationTab)
    {}

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, new ParentActive(imposterSetting));

		IRoleAbility.CreateAbilityCountOption(factory, 3, 50, 5.0f);

		factory.CreateFloatOption(
			Option.Acceleration,
			1.25f, 0.05f, 2.0f, 0.05f);
		factory.CreateIntOption(
			Option.MaxSpeed,
			10, 5, 50, 1);
		factory.CreateFloatOption(
			Option.Friction,
			0.25f, -1.0f, 1.0f, 0.01f);
		var eOpt = factory.CreateBoolOption(
			Option.UseE, true);
		factory.CreateFloatOption(
			Option.EValue,
			0.9f, 0.0f, 2.0f, 0.01f,
			new InvertActive(eOpt));
		factory.CreateFloatOption(
			Option.CanUseSpeed,
			2.0f, 0.0f, 50.0f, 0.1f);
	}

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();
		this.status = new SkaterStatus(
			this.Loader.GetValue<Option, float>(Option.CanUseSpeed) *
			SkaterSkateBehaviour.SpeedOffset);
    }

	public void IntroBeginSetUp()
	{ }

	public void IntroEndSetUp()
	{
		if (this.status is null)
		{
			return;
		}

		this.status.Behaviour = PlayerControl.LocalPlayer.gameObject.AddComponent<SkaterSkateBehaviour>();
		
		var loader = this.Loader;
		var param = new SkaterSkateBehaviour.Parameter(
			loader.GetValue<Option, float>(Option.Friction),
			loader.GetValue<Option, float>(Option.Acceleration),
			loader.GetValue<Option, int>(Option.MaxSpeed),
			loader.GetValue<Option, bool>(Option.UseE) ? loader.GetValue<Option, float>(Option.EValue) : null);

		this.status.Behaviour.Initialize(param);
		this.setBehaviourEnable(false);
	}
	public void AllReset(PlayerControl rolePlayer)
	{
		this.setBehaviourEnable(false);
	}

	public bool UseAbility()
	{
		this.setBehaviourEnable(true);
		return true;
	}

	public bool IsAbilityUse()
		=> IRoleAbility.IsCommonUse() && this.status is not null && this.status.EnableUseButton;

	public void CreateAbility()
	{
		this.CreatePassiveAbilityButton(
			"SkaterSkateOn", "SkaterSkateOff",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.SkaterSkateOn),
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.SkaterSkateOff),
			this.CleanUp);

		if (this.IsCrewmate())
		{
			this.Button?.SetLabelToCrewmate();
		}
	}

	public void CleanUp()
	{
		this.setBehaviourEnable(false);
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{ }

	public void ResetOnMeetingStart()
	{
		this.setBehaviourEnable(false);
	}

	private void setBehaviourEnable(bool enable)
	{
		if (this.status?.Behaviour == null)
		{
			return;
		}

		this.status.Behaviour.enabled = enable;
		this.status.Behaviour.Reset();
	}
}

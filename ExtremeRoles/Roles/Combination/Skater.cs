using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Combination;

public sealed class SkaterManager : FlexibleCombinationRoleManagerBase
{
    public SkaterManager() : base(
		CombinationRoleType.Skater, new Skater(), 1)
    { }

}

#nullable enable

public sealed class Skater :
	MultiAssignRoleBase,
	IRoleAutoBuildAbility,
	IRoleSpecialSetUp,
	IRoleSpecialReset,
	IRoleUsableOverride
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
        string.Concat(this.roleNamePrefix, this.RawRoleName);

	public bool EnableUseButton
		=> this.behaviour == null || !this.behaviour.enabled || this.behaviour.PrevForce.magnitude <= this.canUseSpeed;

	public bool EnableVentButton
		=> this.behaviour == null || !this.behaviour.enabled || this.behaviour.PrevForce.magnitude <= this.canUseSpeed;

	public ExtremeAbilityButton? Button { get; set; }

	private SkaterSkateBehaviour.Parameter param;
	private SkaterSkateBehaviour? behaviour;
	private float canUseSpeed = 0.0f;

    private string roleNamePrefix = string.Empty;

    public Skater(
        ) : base(
            ExtremeRoleId.Skater,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Skater.ToString(),
            ColorPalette.SkaterMizuiro,
            false, true, false, false,
            tab: OptionTab.CombinationTab)
    {}

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
		var imposterSetting = factory.Get((int)CombinationRoleCommonOption.IsAssignImposter);
		CreateKillerOption(factory, imposterSetting);

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
			eOpt,
			invert: true);
		factory.CreateFloatOption(
			Option.CanUseSpeed,
			2.0f, 0.0f, 50.0f, 0.1f);
	}

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();

		var loader = this.Loader;
		this.param = new SkaterSkateBehaviour.Parameter(
			loader.GetValue<Option, float>(Option.Friction),
			loader.GetValue<Option, float>(Option.Acceleration),
			loader.GetValue<Option, int>(Option.MaxSpeed),
			loader.GetValue<Option, bool>(Option.UseE) ? loader.GetValue<Option, float>(Option.EValue) : null);
		this.canUseSpeed =
			loader.GetValue<Option, float>(Option.CanUseSpeed) *
			SkaterSkateBehaviour.SpeedOffset;
    }

	public void IntroBeginSetUp()
	{ }

	public void IntroEndSetUp()
	{
		this.behaviour = PlayerControl.LocalPlayer.gameObject.AddComponent<SkaterSkateBehaviour>();
		this.behaviour.Initialize(this.param);
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
		=> IRoleAbility.IsCommonUse() && this.EnableUseButton;

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
		if (this.behaviour == null) { return; }

		this.behaviour.enabled = enable;
		this.behaviour.Reset();
	}
}

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;

namespace ExtremeRoles.Roles.Combination;

public sealed class SkaterManager : FlexibleCombinationRoleManagerBase
{
    public SkaterManager() : base(new Skater(), 1)
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
            tab: OptionTab.Combination)
    {}

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        var imposterSetting = OptionManager.Instance.Get<bool>(
            GetManagerOptionId(CombinationRoleCommonOption.IsAssignImposter));
        CreateKillerOption(imposterSetting);

		this.CreateAbilityCountOption(parentOps, 3, 50, 5.0f);

		CreateFloatOption(
			Option.Acceleration,
			1.25f, 0.05f, 2.0f, 0.05f,
			parentOps);
		CreateIntOption(
			Option.MaxSpeed,
			10, 5, 50, 1,
			parentOps);
		CreateFloatOption(
			Option.Friction,
			0.25f, -1.0f, 1.0f, 0.01f,
			parentOps);
		var eOpt = CreateBoolOption(
			Option.UseE, true, parentOps);
		CreateFloatOption(
			Option.EValue,
			0.9f, 0.0f, 2.0f, 0.01f,
			eOpt,
			invert: true,
			enableCheckOption: parentOps);
		CreateFloatOption(
			Option.CanUseSpeed,
			2.0f, 0.0f, 50.0f, 0.1f,
			parentOps);
	}

    protected override void RoleSpecificInit()
    {
        this.roleNamePrefix = this.CreateImpCrewPrefix();

		var opt = OptionManager.Instance;
		this.param = new SkaterSkateBehaviour.Parameter(
			opt.GetValue<float>(this.GetRoleOptionId(Option.Friction)),
			opt.GetValue<float>(this.GetRoleOptionId(Option.Acceleration)),
			opt.GetValue<int>(this.GetRoleOptionId(Option.MaxSpeed)),
			opt.GetValue<bool>(this.GetRoleOptionId(Option.UseE)) ? opt.GetValue<float>(this.GetRoleOptionId(Option.EValue)) : null);
		this.canUseSpeed =
			opt.GetValue<float>(this.GetRoleOptionId(Option.CanUseSpeed)) *
			SkaterSkateBehaviour.SpeedOffset;
    }

	public void IntroBeginSetUp()
	{ }

	public void IntroEndSetUp()
	{
		this.behaviour = CachedPlayerControl.LocalPlayer.PlayerControl.gameObject.AddComponent<SkaterSkateBehaviour>();
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
			Loader.CreateSpriteFromResources(
			   Path.SkaterSkateOn),
			Loader.CreateSpriteFromResources(
			   Path.SkaterSkateOff),
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

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
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

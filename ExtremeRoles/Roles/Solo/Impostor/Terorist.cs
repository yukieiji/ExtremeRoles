using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Extension.Il2Cpp;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Terorist : SingleRoleBase, IRoleAbility
{
    public enum TeroristOption
	{
        CanActiveOtherSabotage,
    }

    public ExtremeAbilityButton? Button { get; set; }

	private TeroristTeroSabotageSystem? teroSabo;
	private SabotageSystemType? saboSystem;
	private bool canActiveOtherSabotage;

    public Terorist() : base(
        ExtremeRoleId.Terorist,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Terorist.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
			"TeroristTero",
			FastDestroyableSingleton<HudManager>.Instance.SabotageButton.graphic.sprite);
    }

    public bool IsAbilityUse()
    {
		if (this.teroSabo is null || this.saboSystem == null) { return false; }

        return this.IsCommonUse() && !this.teroSabo.IsActive &&
			(this.canActiveOtherSabotage || !this.saboSystem.AnyActive);
    }

    public bool UseAbility()
    {
		ExtremeSystemTypeManager.RpcUpdateSystem(
			TeroristTeroSabotageSystem.SystemType,
			x =>
			{
				x.Write((byte)TeroristTeroSabotageSystem.Ops.Setup);
			});
		return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
		CreateBoolOption(
			TeroristOption.CanActiveOtherSabotage,
			false, parentOps);
        this.CreateAbilityCountOption(
            parentOps, 10, 100);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

		if (CachedShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
			system.IsTryCast<SabotageSystemType>(out var saboSystem))
		{
			this.saboSystem = saboSystem;
		}

		var optionMng = OptionManager.Instance;
		this.canActiveOtherSabotage = optionMng.GetValue<bool>(
			GetRoleOptionId(TeroristOption.CanActiveOtherSabotage));


		this.teroSabo = new TeroristTeroSabotageSystem(120.0f, 3, 5.0f, !this.canActiveOtherSabotage);
		ExtremeSystemTypeManager.Instance.TryAdd(
			TeroristTeroSabotageSystem.SystemType, this.teroSabo);
	}

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }
}

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Terorist : SingleRoleBase, IRoleAbility
{
    public enum TeroristOption
	{
        CanActiveOtherSabotage,
		ExplosionTime,
		BombNum,
		PlayerActivateTime,
		CanUseDeadPlayer,
		DeadPlayerActivateTime,
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
			Translation.GetString("TeroristBombSet"),
			Loader.CreateSpriteFromResources(
			   Path.TeroristTeroSabotageButton));
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
        this.CreateAbilityCountOption(
            parentOps, 5, 100);
		CreateBoolOption(
			TeroristOption.CanActiveOtherSabotage,
			false, parentOps);
		CreateFloatOption(
			TeroristOption.ExplosionTime,
			45.0f, 10.0f, 240.0f, 1.0f,
			parentOps, format: OptionUnit.Second);
		CreateIntOption(
			TeroristOption.BombNum,
			3, 1, 6, 1, parentOps);
		CreateFloatOption(
			TeroristOption.PlayerActivateTime,
			3.0f, 0.25f, 10.0f, 0.25f,
			parentOps, format: OptionUnit.Second);
		var deadPlayerOpt = CreateBoolOption(
			TeroristOption.CanUseDeadPlayer,
			false, parentOps);
		CreateFloatOption(
			TeroristOption.DeadPlayerActivateTime,
			10.0f, 3.0f, 45.0f, 1.0f,
			deadPlayerOpt, format: OptionUnit.Second);
	}

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

		if (CachedShipStatus.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
			system.IsTryCast<SabotageSystemType>(out var saboSystem))
		{
			this.saboSystem = saboSystem;
		}

		var optionMng = OptionManager.Instance;
		this.canActiveOtherSabotage = optionMng.GetValue<bool>(
			GetRoleOptionId(TeroristOption.CanActiveOtherSabotage));


		var miniGameOption = new TeroristTeroSabotageSystem.MinigameOption(
			optionMng.GetValue<float>(
				GetRoleOptionId(TeroristOption.PlayerActivateTime)),
			optionMng.GetValue<bool>(
				GetRoleOptionId(TeroristOption.CanUseDeadPlayer)),
			optionMng.GetValue<float>(
				GetRoleOptionId(TeroristOption.DeadPlayerActivateTime)));

		var sabotageOption = new TeroristTeroSabotageSystem.Option(
			optionMng.GetValue<float>(
				GetRoleOptionId(TeroristOption.ExplosionTime)),
			optionMng.GetValue<int>(
				GetRoleOptionId(TeroristOption.BombNum)),
			miniGameOption);


		this.teroSabo = new TeroristTeroSabotageSystem(sabotageOption, !this.canActiveOtherSabotage);
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

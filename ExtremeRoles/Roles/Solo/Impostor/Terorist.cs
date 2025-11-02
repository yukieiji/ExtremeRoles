using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Ability;
using UnityEngine;
using ExtremeRoles.Module.CustomOption.Factory;



#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Terorist : SingleRoleBase, IRoleAutoBuildAbility
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
		RoleCore.BuildImpostor(ExtremeRoleId.Terorist),
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
			Tr.GetString("TeroristBombSet"),
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Terorist));
    }

    public bool IsAbilityUse()
    {
		if (this.teroSabo is null || this.saboSystem == null) { return false; }

        return IRoleAbility.IsCommonUse() && !this.teroSabo.IsActive &&
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
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(
            factory, 5, 100);
		factory.CreateBoolOption(
			TeroristOption.CanActiveOtherSabotage,
			false);
		factory.CreateFloatOption(
			TeroristOption.ExplosionTime,
			45.0f, 10.0f, 240.0f, 1.0f,
			format: OptionUnit.Second);
		factory.CreateIntOption(
			TeroristOption.BombNum,
			3, 1, 6, 1);
		factory.CreateFloatOption(
			TeroristOption.PlayerActivateTime,
			3.0f, 0.25f, 10.0f, 0.25f,
			format: OptionUnit.Second);
		var deadPlayerOpt = factory.CreateBoolOption(
			TeroristOption.CanUseDeadPlayer,
			false);
		factory.CreateFloatOption(
			TeroristOption.DeadPlayerActivateTime,
			10.0f, 3.0f, 45.0f, 1.0f,
			deadPlayerOpt, format: OptionUnit.Second);
	}

    protected override void RoleSpecificInit()
    {
		if (ShipStatus.Instance != null &&
			ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Sabotage, out var system) &&
			system.IsTryCast<SabotageSystemType>(out var saboSystem))
		{
			this.saboSystem = saboSystem;
		}

		var cate = this.Loader;
		this.canActiveOtherSabotage = cate.GetValue<TeroristOption, bool>(
			TeroristOption.CanActiveOtherSabotage);


		var miniGameOption = new TeroristTeroSabotageSystem.MinigameOption(
			cate.GetValue<TeroristOption, float>(
				TeroristOption.PlayerActivateTime),
			cate.GetValue<TeroristOption, bool>(
				TeroristOption.CanUseDeadPlayer),
			cate.GetValue<TeroristOption, float>(
				TeroristOption.DeadPlayerActivateTime));

		var sabotageOption = new TeroristTeroSabotageSystem.Option(
			cate.GetValue<TeroristOption, float>(
				TeroristOption.ExplosionTime),
			cate.GetValue<TeroristOption, int>(
				TeroristOption.BombNum),
			miniGameOption);

		this.teroSabo = ExtremeSystemTypeManager.Instance.CreateOrGet(
			TeroristTeroSabotageSystem.SystemType,
			()=> new TeroristTeroSabotageSystem(sabotageOption, !this.canActiveOtherSabotage));
	}

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }
}

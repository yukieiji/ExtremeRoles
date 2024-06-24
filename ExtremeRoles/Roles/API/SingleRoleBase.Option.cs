using ExtremeRoles.Module.CustomOption;

using ExtremeRoles.Module.NewOption.Factory;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Interfaces;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    protected sealed override void CreateKillerOption(
        AutoParentSetOptionCategoryFactory factory,
		IOption parent = null,
		bool ignorePrefix = true)
    {
        var killCoolOption = factory.CreateBoolOption(
            KillerCommonOption.HasOtherKillCool,
            false, parent,
			ignorePrefix: ignorePrefix);
		factory.CreateFloatOption(
            KillerCommonOption.KillCoolDown,
            30f, 1.0f, 120f, 0.5f,
            killCoolOption, format: OptionUnit.Second,
			ignorePrefix: ignorePrefix);

        var killRangeOption = factory.CreateBoolOption(
            KillerCommonOption.HasOtherKillRange,
            false, parent,
			ignorePrefix: ignorePrefix);
		factory.CreateSelectionOption(
            KillerCommonOption.KillRange,
            OptionCreator.Range,
            killRangeOption,
			ignorePrefix: ignorePrefix);
    }
    protected sealed override AutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		var factory = NewOptionManager.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetRoleGroupId(this.Id),
			this.RawRoleName, this.Tab, this.NameColor);

		var roleSetOption = factory.CreateSelectionOption(
            RoleCommonOption.SpawnRate,
            OptionCreator.SpawnRate,
			format: OptionUnit.Percentage,
			ignorePrefix: true);

        int spawnNum = this.IsImpostor() ?
            GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

		factory.CreateIntOption(
            RoleCommonOption.RoleNum,
            1, 1, spawnNum, 1,
			ignorePrefix: true);

		factory.CreateIntOption(
			RoleCommonOption.AssignWeight,
			500, 1, 1000, 1,
			ignorePrefix: true);

        return factory;
    }

    protected sealed override void CreateVisionOption(
        AutoParentSetOptionCategoryFactory factory, bool ignorePrefix = true)
    {
        var visionOption = factory.CreateBoolOption(
            RoleCommonOption.HasOtherVision,
            false,
			ignorePrefix: ignorePrefix);
		factory.CreateFloatOption(RoleCommonOption.Vision,
            2f, 0.25f, 5.0f, 0.25f,
            visionOption, format: OptionUnit.Multiplier,
			ignorePrefix: ignorePrefix);

		factory.CreateBoolOption(
            RoleCommonOption.ApplyEnvironmentVisionEffect,
            this.IsCrewmate(), visionOption,
			ignorePrefix: ignorePrefix);
    }
}

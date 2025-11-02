using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    protected sealed override AutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		var factory = OptionCategoryAssembler.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetRoleGroupId(this.Core.Id),
			this.Core.Name, this.Tab, this.Core.Color);

		var roleSetOption = factory.Create0To100Percentage10StepOption(
            RoleCommonOption.SpawnRate,
			ignorePrefix: true);

        int spawnNum = this.IsImpostor() ?
            GameSystem.MaxImposterNum : GameSystem.VanillaMaxPlayerNum - 1;

		factory.CreateNewIntOption(
            RoleCommonOption.RoleNum,
            1, 1, spawnNum, 1,
			ignorePrefix: true);

		factory.CreateNewIntOption(
			RoleCommonOption.AssignWeight,
			500, 1, 1000, 1,
			ignorePrefix: true);

        return factory;
    }

    protected sealed override void CreateVisionOption(
        AutoParentSetOptionCategoryFactory factory, bool ignorePrefix = true)
    {
        var visionOption = factory.CreateNewBoolOption(
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

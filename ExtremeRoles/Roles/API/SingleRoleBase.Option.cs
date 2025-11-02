using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory.Old;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.Roles.API;

public abstract partial class SingleRoleBase
{
    protected sealed override OldAutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		var factory = OldOptionManager.CreateAutoParentSetOptionCategory(
			ExtremeRoleManager.GetRoleGroupId(this.Core.Id),
			this.Core.Name, this.Tab, this.Core.Color);

		var roleSetOption = factory.Create0To100Percentage10StepOption(
            RoleCommonOption.SpawnRate,
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
        OldAutoParentSetOptionCategoryFactory factory, bool ignorePrefix = true)
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

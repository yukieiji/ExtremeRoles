

using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Roles.API;


public interface IRoleSpecificOption
{

}

public class CommonOption(RoleCore core, IOptionLoader loader)
{
	private readonly IOptionLoader loader = loader;
	private readonly RoleCore core = core;

	public bool HasOtherVison => loader.GetValue<RoleCommonOption, bool>(RoleCommonOption.HasOtherVision);

	public float Vison => this.HasOtherVison ?
		loader.GetValue<RoleCommonOption, float>(RoleCommonOption.Vision) : DefaultVison;
	public bool IsApplyEnvironmentVision => this.HasOtherVison ?
		loader.GetValue<RoleCommonOption, bool>(RoleCommonOption.ApplyEnvironmentVisionEffect) :
		DefaultIsApplyEnvironmentVision;


	private float DefaultVison => core.Team is ExtremeRoleType.Impostor ?
		GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod) :
		GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.CrewLightMod);
	private bool DefaultIsApplyEnvironmentVision => core.Team is not ExtremeRoleType.Impostor;

	public bool HasOtherKillCool => loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillCool);
	public float KillCool => HasOtherKillCool ?
		loader.GetValue<KillerCommonOption, float>(KillerCommonOption.KillCoolDown) : 
		Player.DefaultKillCoolTime;

	public bool HasOtherKillRange => loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillRange);
	public int KillRange => HasOtherKillRange ?
		loader.GetValue<KillerCommonOption, int>(KillerCommonOption.KillRange) :
		GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
}

public sealed record RoleOption<T>(CommonOption Common, T Role) where T : IRoleSpecificOption;

public interface IRoleOptionFactory
{
	// public RoleCore Core { get; }

	public void Build(AutoParentSetOptionCategoryFactory factory);
}

public interface ISpecificOptionLoader<T> where T : IRoleSpecificOption
{
	public T Load(IOptionLoader loader);
}

public interface IRoleOptionProvider
{
	public RoleOption<T> Provide<T>(RoleCore core) where T : IRoleSpecificOption;
}

public abstract partial class SingleRoleBase
{
    protected sealed override AutoParentSetOptionCategoryFactory CreateSpawnOption()
    {
		var factory = OptionManager.CreateAutoParentSetOptionCategory(
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

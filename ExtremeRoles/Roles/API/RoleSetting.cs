using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API.Interface;
using System;

namespace ExtremeRoles.Roles.API;


public enum ExtremeRoleType : int
{
    Null = -2,
    Neutral = -1,
    Crewmate = 0,
    Impostor = 1
}
public enum RoleCommonOption
{
	SpawnRate = 50,
	RoleNum,
    AssignWeight,
    HasOtherVision,
    Vision,
    ApplyEnvironmentVisionEffect,
}
public enum KillerCommonOption
{
    HasOtherKillRange = 60,
    KillRange,
    HasOtherKillCool,
    KillCoolDown,
}

public abstract class RoleOptionBase
{
    public virtual bool CanKill { get; set; } = false;

	public abstract IOptionLoader Loader { get; }

	public void Initialize()
    {
        CommonInit();
        RoleSpecificInit();

		//TODO : 消して動くかチェック
		if (this is IRoleAbility ability)
		{
			ability.RoleAbilityInit();
		}
    }

    public void CreateRoleAllOption(AutoRoleOptionCategoryFactory factory)
    {
		using var cate = CreateSpawnOption(factory);
		this.CreateRoleSpecificOption(cate);
    }
    public void CreateRoleSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope, bool ignorePrefix = true)
    {
        CreateVisionOption(categoryScope.Builder, ignorePrefix);

        if (this.CanKill)
        {
            CreateKillerOption(categoryScope.Builder, ignorePrefix: ignorePrefix);
        }

        CreateSpecificOption(categoryScope);
    }
    protected abstract OptionCategoryScope<AutoParentSetBuilder> CreateSpawnOption(AutoRoleOptionCategoryFactory factory);

    protected abstract void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope);
    protected abstract void CreateVisionOption(AutoParentSetBuilder factory, bool ignorePrefix = true);

    protected abstract void CommonInit();

    protected abstract void RoleSpecificInit();

	protected static void CreateKillerOption(
		AutoParentSetBuilder factory,
		IOption parent = null,
		bool ignorePrefix = true,
		bool invert = false)
	{
		var killCoolOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillCool,
			false, parent,
			ignorePrefix: ignorePrefix,
			invert: invert);
		factory.CreateFloatOption(
			KillerCommonOption.KillCoolDown,
			30f, 1.0f, 120f, 0.5f,
			killCoolOption, format: OptionUnit.Second,
			ignorePrefix: ignorePrefix);

		var killRangeOption = factory.CreateBoolOption(
			KillerCommonOption.HasOtherKillRange,
			false, parent,
			ignorePrefix: ignorePrefix,
			invert: invert);
		factory.CreateSelectionOption(
			KillerCommonOption.KillRange,
			OptionCreator.Range,
			killRangeOption,
			ignorePrefix: ignorePrefix);
	}

	protected static void EnumCheck<T>(T isEnum) where T : struct, IConvertible
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
        {
            throw new ArgumentException(nameof(T));
        }
    }
}

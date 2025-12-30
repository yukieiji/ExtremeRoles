using System;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Module.PRNG;
using ExtremeRoles.Module.RNG;

namespace ExtremeRoles;

public static class RandomGenerator
{
    private static RngSelector selector;

    public static RNGBase Instance
    {
        get
        {
            if (selector == null)
            {
                selector = new RngSelector();
            }
            return selector.Instance;
        }
    }

    public static bool IsUsingStrongGenerator => OptionManager.Instance.TryGetCategory(
            OptionTab.GeneralTab,
            (int)OptionCreator.CommonOption.RandomOption,
            out var category) &&
        category.GetValue<bool>((int)OptionCreator.RandomOptionKey.UseStrong);

    public static void Initialize()
    {
        if (selector == null)
        {
            selector = new RngSelector();
        }
        selector.Initialize();

        var sample = Instance.Next();

        Logging.Debug($"UsePRNG:{Instance}");
        Logging.Debug($"Sample OutPut:{sample}");
    }

    public static Random GetTempGenerator()
    {
        if (IsUsingStrongGenerator)
        {
            return new Random(SeedInfo.CreateStrongRandomSeed());
        }
        else
        {
            return new Random(SeedInfo.CreateNormalRandomSeed());
        }
    }
}

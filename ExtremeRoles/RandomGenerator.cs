using ExtremeRoles.Helper;
using ExtremeRoles.Module.PRNG;

#nullable enable

namespace ExtremeRoles;

public static class RandomGenerator
{
    private static RngSelector? selector;

    public static IRng Instance
    {
        get
        {
            if (selector is null)
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
        if (selector is null)
        {
            selector = new RngSelector();
        }
        selector.Initialize();

        int sample = Instance.Next();

        Logging.Debug($"UsePRNG:{Instance}");
        Logging.Debug($"Sample OutPut:{sample}");
    }
}

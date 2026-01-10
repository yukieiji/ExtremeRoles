using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Core.Service;
using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles;

public static class RandomGenerator
{
    private static RngSelector? selector;

    public static IRng Instance
    {
        get
        {
			selector ??= new RngSelector();
			return selector.Instance;
        }
    }

	public static bool IsUsingStrongGenerator => selector is not null && selector.IsStrong;

    public static void Initialize()
    {
		selector ??= new RngSelector();
		selector.Initialize();

        int sample = Instance.Next();

        Logging.Debug($"UsePRNG:{Instance}");
        Logging.Debug($"Sample OutPut:{sample}");
    }
}

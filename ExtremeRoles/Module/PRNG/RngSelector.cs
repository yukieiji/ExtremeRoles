using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class RngSelector
{

    public IRng Instance { get; private set; }

    private OptionInfo prev;

    private const int randCategoryKey = (int)OptionCreator.CommonOption.RandomOption;
    private const int useStrongKey = (int)OptionCreator.RandomOptionKey.UseStrong;
    private const int algorithmKey = (int)OptionCreator.RandomOptionKey.Algorithm;

	private readonly record struct OptionInfo(bool UseStrongGen, int Selection)
	{
		public OptionInfo(OptionCategory category) : this(
			category.GetValue<bool>(useStrongKey),
			category.GetValue<int>(algorithmKey))
		{

		}
	}

    public RngSelector()
    {
        using var seed = new SeedInfo();

		if (!OptionManager.Instance.TryGetCategory(
				OptionTab.GeneralTab,
				randCategoryKey,
				out var category))
		{
			throw new System.InvalidOperationException();
		}

		this.prev = new OptionInfo(category);
		Instance = createGlobalRandomGenerator(seed, this.prev);
    }

    public void Initialize()
    {
        if (!hasOptionChanged(out var newInfo))
        {
			return;
        }
		using var seed = new SeedInfo();
		if (Instance == null || newInfo.UseStrongGen != this.prev.UseStrongGen)
		{
			Instance = createGlobalRandomGenerator(seed, newInfo);
		}
		else if (newInfo.Selection != this.prev.Selection)
		{
			Instance = createStrongRng(seed, newInfo.Selection);
		}
		this.prev = newInfo;
	}

    private bool hasOptionChanged(out OptionInfo info)
    {
        if (!OptionManager.Instance.TryGetCategory(
                OptionTab.GeneralTab,
                randCategoryKey,
                out var category) ||
			category is null)
        {
			info = new OptionInfo();
            return false;
        }

		info = new OptionInfo(category);

        return Instance is null || info.UseStrongGen != prev.UseStrongGen || info.Selection != prev.Selection;
    }

    private IRng createGlobalRandomGenerator(SeedInfo seed, in OptionInfo info)
    {
		IRng rng;
        Logging.Debug("Initialize RNG");
        if (info.UseStrongGen)
        {
            rng = createStrongRng(seed, info.Selection);
        }
        else
        {
            rng = new SystemRandomWrapper(seed.CreateNormal());
            UnityEngine.Random.InitState(seed.CreateNormal());
        }
		return rng;
    }

    private IRng createStrongRng(SeedInfo seed, int selection)
    {
		IRng rng = getAditionalPrng(seed, selection);
        UnityEngine.Random.InitState(seed.CreateInt());
		return rng;
    }

    private static IRng getAditionalPrng(SeedInfo seed, int selection)
    {
        switch (selection)
        {
            case 0:
                return new Pcg32XshRr(seed);
            case 1:
                return new Pcg64RxsMXs(seed);
            case 2:
                return new Xorshift64(seed);
            case 3:
                return new Xorshift128(seed);
            case 4:
                return new Xorshiro256StarStar(seed);
            case 5:
                return new Xorshiro512StarStar(seed);
            case 6:
                return new RomuMono(seed);
            case 7:
                return new RomuTrio(seed);
            case 8:
                return new RomuQuad(seed);
            case 9:
                return new Seiran128(seed);
            case 10:
                return new Shioi128(seed);
            case 11:
                return new JFT32(seed);
            default:
                return new SystemRandomWrapper(seed.CreateInt());
        }
    }
}

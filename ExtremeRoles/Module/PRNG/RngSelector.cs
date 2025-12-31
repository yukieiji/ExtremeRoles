using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.PRNG;

public sealed class RngSelector
{
    public IRng Instance { get; private set; }

    private bool prevValue;
    private int prevSelection;

    private const int randCategoryKey = (int)OptionCreator.CommonOption.RandomOption;
    private const int useStrongKey = (int)OptionCreator.RandomOptionKey.UseStrong;
    private const int algorithmKey = (int)OptionCreator.RandomOptionKey.Algorithm;

    public RngSelector()
    {
		using var seed = new SeedInfo();
        createGlobalRandomGenerator(
			seed,
            OptionManager.Instance.TryGetCategory(
                OptionTab.GeneralTab,
                randCategoryKey,
                out var category) &&
            category.GetValue<bool>(useStrongKey));
    }

    public void Initialize()
    {
        if (!OptionManager.Instance.TryGetCategory(
                OptionTab.GeneralTab,
                randCategoryKey,
                out var category))
        {
            return;
        }

		bool useStrongGen = category.GetValue<bool>(useStrongKey);
        if (Instance != null)
        {
            if (useStrongGen != prevValue)
			{
				using var seed = new SeedInfo();
				createGlobalRandomGenerator(new SeedInfo(), useStrongGen);
            }
            else
            {
                int selection = category.GetValue<int>(algorithmKey);
                if (prevSelection != selection)
                {
					using var seed = new SeedInfo();
					createStrongRng(category, seed);
                }
            }
        }
        else
        {
			using var seed = new SeedInfo();
			createGlobalRandomGenerator(seed, useStrongGen);
        }
    }

    private void createGlobalRandomGenerator(SeedInfo seed, bool isStrong)
    {
        Logging.Debug("Initialize RNG");
        if (OptionManager.Instance.TryGetCategory(
                OptionTab.GeneralTab,
                randCategoryKey,
                out var category) &&
            isStrong)
        {
			createStrongRng(category, seed);
        }
        else
        {
            Instance = new SystemRandomWrapper(seed.CreateNormal());
            UnityEngine.Random.InitState(seed.CreateNormal());
            prevSelection = -1;
        }
        prevValue = isStrong;
    }

	private void createStrongRng(OptionCategory category, SeedInfo seed)
	{
		int selection = category.GetValue<int>(algorithmKey);
		Instance = getAditionalPrng(seed, selection);
		UnityEngine.Random.InitState(seed.CreateInt());
		prevSelection = selection;
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

using ExtremeRoles.Helper;

namespace ExtremeRoles.Module.PRNG;

public sealed class RngSelector
{
    public RNGBase Instance { get; private set; }

    private bool prevValue;
    private int prevSelection;

    private const int randCategoryKey = (int)OptionCreator.CommonOption.RandomOption;
    private const int useStrongKey = (int)OptionCreator.RandomOptionKey.UseStrong;
    private const int algorithmKey = (int)OptionCreator.RandomOptionKey.Algorithm;

    public RngSelector()
    {
        createGlobalRandomGenerator(
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
                createGlobalRandomGenerator(useStrongGen);
            }
            else
            {
                int selection = category.GetValue<int>(algorithmKey);
                if (prevSelection != selection)
                {
                    Instance = getAditionalPrng(selection);
                    UnityEngine.Random.InitState(SeedInfo.CreateStrongRandomSeed());
                    prevSelection = selection;
                }
            }
        }
        else
        {
            createGlobalRandomGenerator(useStrongGen);
        }
    }

    private void createGlobalRandomGenerator(bool isStrong)
    {
        Logging.Debug("Initialize RNG");
        if (OptionManager.Instance.TryGetCategory(
                OptionTab.GeneralTab,
                randCategoryKey,
                out var category) &&
            isStrong)
        {
            int selection = category.GetValue<int>(algorithmKey);
            Instance = getAditionalPrng(selection);
            UnityEngine.Random.InitState(SeedInfo.CreateStrongRandomSeed());
            prevSelection = selection;
        }
        else
        {
            Instance = new SystemRandomWrapper(0, 0);
            UnityEngine.Random.InitState(SeedInfo.CreateNormalRandomSeed());
            prevSelection = -1;
        }
        prevValue = isStrong;
    }

    private static RNGBase getAditionalPrng(int selection)
    {
        switch (selection)
        {
            case 0:
                return new Pcg32XshRr(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 1:
                return new Pcg64RxsMXs(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 2:
                return new Xorshift64(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 3:
                return new Xorshift128(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 4:
                return new Xorshiro256StarStar(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 5:
                return new Xorshiro512StarStar(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 6:
                return new RomuMono(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 7:
                return new RomuTrio(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 8:
                return new RomuQuad(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 9:
                return new Seiran128(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 10:
                return new Shioi128(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            case 11:
                return new JFT32(
                    SeedInfo.CreateLongStrongSeed(),
                    SeedInfo.CreateLongStrongSeed());
            default:
                return new SystemRandomWrapper(0, 0);
        }
    }
}

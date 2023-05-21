namespace ExtremeRoles.Module.PRNG;

public sealed class Pcg64RxsMXs : RNG64Base
{
    // 以下のURLを元に実装
    // https://github.com/Shiroechi/Litdex.Security.RNG/blob/main/Source/Security/RNG/PRNG/PcgRxsMXs64.cs

    private ulong state;
    private ulong increment = 1442695040888963407ul;

    // This shifted to the left and or'ed with 1ul results in the default increment.
    private const ulong ShiftedIncrement = 721347520444481703ul;
    private const ulong Multiplier = 6364136223846793005ul;

    public Pcg64RxsMXs(
        ulong seed, ulong state = ShiftedIncrement) : base(seed, state)
    { }

    public override ulong NextUInt64()
    {
        ulong oldseed = this.state;
        this.state = (oldseed * Multiplier) + (increment | 1);
        ulong word = ((oldseed >> ((int)(oldseed >> 59) + 5)) ^ oldseed) * 12605985483714917081;
        return (word >> 43) ^ word;
    }

    protected override void Initialize(ulong seed, ulong initStete)
    {
        this.state = (seed + initStete) * Multiplier + initStete;
        this.increment = initStete;
    }
}

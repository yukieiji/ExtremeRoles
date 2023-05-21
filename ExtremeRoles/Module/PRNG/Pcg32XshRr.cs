namespace ExtremeRoles.Module.PRNG;

public sealed class Pcg32XshRr : RNG32Base
{
    private ulong state;
    private ulong increment = 1442695040888963407ul;

    // This shifted to the left and or'ed with 1ul results in the default increment.
    private const ulong ShiftedIncrement = 721347520444481703ul;
    private const ulong Multiplier = 6364136223846793005ul;

    public Pcg32XshRr(
        ulong seed, ulong state = ShiftedIncrement) : base(seed, state)
    { }

    public override uint NextUInt()
    {
        ulong oldState = this.state;
        this.state = unchecked(oldState * Multiplier + this.increment);
        uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
        int rot = (int)(oldState >> 59);
        uint result = (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
        return result;
    }

    protected override void Initialize(ulong seed, ulong initStete)
    {
        this.state = (seed + initStete) * Multiplier + initStete;
        this.increment = initStete;
    }
}

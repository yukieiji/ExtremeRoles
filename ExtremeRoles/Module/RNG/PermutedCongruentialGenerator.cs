namespace ExtremeRoles.Module.RNG
{
    public sealed class PermutedCongruentialGenerator : RNG32Base
    {
        /*
            以下のURLの実装を元に実装
             https://github.com/igiagkiozis/PCGSharp
            
            ToDo:PCG64-XSH-RR => PCG64-RXS-M-XS
        
            implement(official):
                static xtype output(itype internal)
                {
                    constexpr bitcount_t xtypebits = bitcount_t(sizeof(xtype) * 8);
                    constexpr bitcount_t bits = bitcount_t(sizeof(itype) * 8);
                    constexpr bitcount_t opbits = xtypebits >= 128 ? 6
                                             : xtypebits >=  64 ? 5
                                             : xtypebits >=  32 ? 4
                                             : xtypebits >=  16 ? 3
                                             :                    2;
                    constexpr bitcount_t shift = bits - xtypebits;
                    constexpr bitcount_t mask = (1 << opbits) - 1;
                    bitcount_t rshift =
                        opbits ? bitcount_t(internal >> (bits - opbits)) & mask : 0;
                    internal ^= internal >> (opbits + rshift);
                    internal *= mcg_multiplier<itype>::multiplier();
                    xtype result = internal >> shift;
                    result ^= result >> ((2U*xtypebits+2U)/3U);
                    return result;
                }

                static itype unoutput(itype internal)
                {
                    constexpr bitcount_t bits = bitcount_t(sizeof(itype) * 8);
                    constexpr bitcount_t opbits = bits >= 128 ? 6
                                             : bits >=  64 ? 5
                                             : bits >=  32 ? 4
                                             : bits >=  16 ? 3
                                             :               2;
                    constexpr bitcount_t mask = (1 << opbits) - 1;

                    internal = unxorshift(internal, bits, (2U*bits+2U)/3U);

                    internal *= mcg_unmultiplier<itype>::unmultiplier();

                    bitcount_t rshift = opbits ? (internal >> (bits - opbits)) & mask : 0;
                    internal = unxorshift(internal, bits, opbits + rshift);

                    return internal;
                }
        */

        private ulong state;
        private ulong increment = 1442695040888963407ul;

        // This shifted to the left and or'ed with 1ul results in the default increment.
        private const ulong ShiftedIncrement = 721347520444481703ul;
        private const ulong Multiplier = 6364136223846793005ul;

        public PermutedCongruentialGenerator(
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
            this.state = 0ul;
            setStream(initStete);

            NextUInt();

            this.state += seed;

            NextUInt();
        }
        private void setStream(ulong sequence)
        {
            this.increment = (sequence << 1) | 1;
        }
    }
}

namespace ExtremeRoles.Module.PRNG
{
    public sealed class Seiran128 : RNG64Base
    {
        /*
            以下のURLの実装を元に実装
            https://github.com/andanteyk/prng-seiran
             ライセンス：https://creativecommons.org/publicdomain/zero/1.0/
            
        */
        private ulong state0, state1;

        public Seiran128(ulong seed, ulong state) : base(seed, state)
        { }

        public override ulong NextUInt64()
        {
            ulong s0 = state0, s1 = state1;
            ulong result = rotl((s0 + s1) * 9, 29) + s0;

            state0 = s0 ^ rotl(s1, 29);
            state1 = s0 ^ s1 << 9;

            return result;
        }

        protected override void Initialize(ulong seed, ulong initStete)
        {
            state0 = seed;
            state1 = initStete;
            while ((state0 | state1)  == 0)
            {
                state0 = RandomGenerator.CreateLongStrongSeed();
                state1 = RandomGenerator.CreateLongStrongSeed();
            }
            // at least one value must be non-zero
        }
        private ulong rotl(ulong x, int k) => (x << k) | (x >> (-k & 63));
    }
}

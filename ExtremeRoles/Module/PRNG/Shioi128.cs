namespace ExtremeRoles.Module.PRNG
{
    public sealed class Shioi128 : RNG64Base
    {
        /*
            以下のURLの実装を元に実装
             https://github.com/andanteyk/prng-shioi/blob/master/shioi128.c
             ライセンス：https://creativecommons.org/publicdomain/zero/1.0/
            
        */
        private ulong state0, state1;

        public Shioi128(ulong seed, ulong state) : base(seed, state)
        { }

        public override ulong NextUInt64()
        {
            ulong s0 = state0, s1 = state1;
            ulong result = rotl(s0 * 0xD2B74407B1CE6E93, 29) + s1;

            state0 = s1;
            state1 = (s0 << 2) ^ (s0 >> 19) ^ s1;

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

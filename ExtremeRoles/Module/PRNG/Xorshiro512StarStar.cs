namespace ExtremeRoles.Module.PRNG
{
    public sealed class Xorshiro512StarStar : RNG64Base
    {
        /*
            以下のURLの実装を元に実装
             https://github.com/colgreen/Redzen/blob/main/Redzen/Random/Xoshiro512StarStarRandom.cs
            
        */

        private ulong _s0, _s1, _s2, _s3, _s4, _s5, _s6, _s7;

        public Xorshiro512StarStar(
            ulong seed, ulong state) : base(seed, state)
        { }

        public override ulong NextUInt64()
        {
            ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3, s4 = _s4, s5 = _s5, s6 = _s6, s7 = _s7;

            ulong result = LeftOps(s1 * 5, 7) * 9;
            ulong t = s1 << 11;

            s2 ^= s0;
            s5 ^= s1;
            s1 ^= s2;
            s7 ^= s3;
            s3 ^= s4;
            s4 ^= s5;
            s0 ^= s6;
            s6 ^= s7;
            s6 ^= t;

            s7 = LeftOps(s7, 21);

            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;
            _s4 = s4;
            _s5 = s5;
            _s6 = s6;
            _s7 = s7;

            return result;
        }

        protected override void Initialize(ulong seed, ulong initStete)
        {
            _s0 = seed;
            _s1 = initStete;
            do
            {
                _s2 = RandomGenerator.CreateLongStrongSeed();
                _s3 = RandomGenerator.CreateLongStrongSeed();
                _s4 = RandomGenerator.CreateLongStrongSeed();
                _s5 = RandomGenerator.CreateLongStrongSeed();
                _s6 = RandomGenerator.CreateLongStrongSeed();
                _s7 = RandomGenerator.CreateLongStrongSeed();
            } 
            while ((_s2 | _s3 | _s4 | _s5 | _s6 | _s7) == 0); // at least one value must be non-zero
        }
    }
}

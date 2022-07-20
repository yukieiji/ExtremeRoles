namespace ExtremeRoles.Module.PRNG
{
    public sealed class Xorshiro256StarStar : RNG64Base
    {
        /*
            以下のURLの実装を元に実装
             https://source.dot.net/#System.Private.CoreLib/Random.Xoshiro256StarStarImpl.cs,bb77e610694e64ca
            
        */

        private ulong _s0, _s1, _s2, _s3;

        public Xorshiro256StarStar(
            ulong seed, ulong state) : base(seed, state)
        { }

        public override ulong NextUInt64()
        {
            ulong s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

            ulong result = leftOps(s1 * 5, 7) * 9;
            ulong t = s1 << 17;

            s2 ^= s0;
            s3 ^= s1;
            s1 ^= s2;
            s0 ^= s3;

            s2 ^= t;
            s3 = leftOps(s3, 45);

            _s0 = s0;
            _s1 = s1;
            _s2 = s2;
            _s3 = s3;

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
            } 
            while ((_s2 | _s3) == 0); // at least one value must be non-zero
        }
        // BitOperations.Left : https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs
        private ulong leftOps(ulong value, int offset)
            => (value << offset) | (value >> (64 - offset));
    }
}

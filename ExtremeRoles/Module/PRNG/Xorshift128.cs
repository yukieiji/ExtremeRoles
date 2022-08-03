namespace ExtremeRoles.Module.PRNG
{
    public sealed class Xorshift128 : RNG32Base
    {
        /*
            以下のURLの実装を元に実装
             https://ja.wikipedia.org/wiki/Xorshift
            
        */

        private uint _s0, _s1, _s2, _s3;

        public Xorshift128(
            ulong seed, ulong state) : base(seed, state)
        { }

        public override uint NextUInt()
        {
            uint s0 = _s0, s1 = _s1, s2 = _s2, s3 = _s3;

            uint t = s0 ^ (s0 << 0);
            _s0 = s1;
            _s1 = s2;
            _s2 = s3;
            _s3 = (s3 ^ (s3 >> 19)) ^ (t ^ (t >> 8));

            return _s3;
        }

        protected override void Initialize(ulong seed, ulong initStete)
        {
            _s0 = (uint)(seed >> 32);
            _s1 = (uint)(initStete >> 32);
            do
            {
                _s2 = RandomGenerator.CreateStrongSeed();
                _s3 = RandomGenerator.CreateStrongSeed();
            } 
            while ((_s2 | _s3) == 0); // at least one value must be non-zero
        }
    }
}

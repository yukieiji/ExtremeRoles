namespace ExtremeRoles.Module.PRNG
{
    public sealed class RomuTrio : RNG64Base
    {
        /*
            以下のURLの実装を元に実装
             https://arxiv.org/pdf/2002.11331.pdf
            
        */
        private const ulong a = 15241094284759029579ul;
        private ulong xState, yState, zState;

        public RomuTrio(ulong seed, ulong state) : base(seed, state)
        { }

        public override ulong NextUInt64()
        {
            ulong xp = xState, yp = yState, zp = zState;

            xState = a * zp;
            
            yState = yp - xp;
            yState = LeftOps(yState, 12);

            zState = zp - yp;
            zState = LeftOps(zState, 44);

            return xp;
        }

        protected override void Initialize(ulong seed, ulong initStete)
        {
            xState = seed;
            yState = initStete;
            do
            {
                zState = RandomGenerator.CreateLongStrongSeed();
            } 
            while (zState == 0); // at least one value must be non-zero

            while ((xState | yState) == 0)
            {
                xState = RandomGenerator.CreateLongStrongSeed();
                yState = RandomGenerator.CreateLongStrongSeed();
            }
        }
    }
}

using System;

namespace ExtremeRoles.Module.RNG
{
    public sealed class SystemRandomWrapper : RNG32Base
    {

        private Random rand;

        public SystemRandomWrapper(
            ulong seed, ulong state) : base(seed, state)
        { }

        public override int Next() => this.rand.Next();

        public override int Next(int maxExclusive) => this.rand.Next(maxExclusive);

        public override int Next(int minInclusive, int maxExclusive) => this.rand.Next(minInclusive, maxExclusive);

        public override uint NextUInt() => 0;

        protected override void Initialize(ulong seed, ulong initStete)
        {
            this.rand = new Random(
                RandomGenerator.CreateStrongRandomSeed());
        }
    }
}

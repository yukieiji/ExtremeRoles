using System;

namespace ExtremeRoles.Module.PRNG
{
    public abstract class RNGBase
    {
        public RNGBase(ulong seed, ulong state)
        {
            Initialize(seed, state);
        }

        public abstract int Next();

        public abstract int Next(int maxExclusive);

        public abstract int Next(int minInclusive, int maxExclusive);

        protected abstract void Initialize(ulong seed, ulong initStete);
    }

    public abstract class RNG32Base : RNGBase
    {
        public RNG32Base(ulong seed, ulong state) : base (seed, state)
        { }

        public override int Next()
        {
            while (true)
            {
                // Get top 31 bits to get a value in the range [0, int.MaxValue], but try again
                // if the value is actually int.MaxValue, as the method is defined to return a value
                // in the range [0, int.MaxValue).
                uint result = NextUInt() >> 1;
                if (result != int.MaxValue)
                {
                    return (int)result;
                }
            }
        }

        public override int Next(int maxExclusive)
        {
            // Backport .Net6 Round logic
            // from https://source.dot.net/#System.Private.CoreLib/Random.Xoshiro256StarStarImpl.cs,bb77e610694e64ca

            if (maxExclusive <= 0)
            {
                throw new ArgumentException("Max Exclusive must be positive");
            }

            if (maxExclusive == 1)
            {
                return 0;
            }

            int bits = (int)Math.Ceiling(Math.Log(maxExclusive, 2));
            while (true)
            {
                uint result = NextUInt() >> (sizeof(uint) * 8 - bits);
                if (result < (uint)maxExclusive)
                {
                    return (int)result;
                }
            }
        }

        public override int Next(int minInclusive, int maxExclusive)
        {

            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentException("MaxExclusive must be larger than MinInclusive");
            }

            int range = (maxExclusive - minInclusive);
            return Next(range) + minInclusive;
        }

        public abstract uint NextUInt();

    }
    public abstract class RNG64Base : RNGBase
    {
        public RNG64Base(ulong seed, ulong state) : base(seed, state)
        { }

        public override int Next()
        {
            while (true)
            {
                // Get top 31 bits to get a value in the range [0, int.MaxValue], but try again
                // if the value is actually int.MaxValue, as the method is defined to return a value
                // in the range [0, int.MaxValue).
                ulong result = NextUInt64() >> 33;
                if (result != int.MaxValue)
                {
                    return (int)result;
                }
            }
        }

        public override int Next(int maxExclusive)
        {
            // Backport .Net6 Round logic
            // from https://source.dot.net/#System.Private.CoreLib/Random.Xoshiro256StarStarImpl.cs,bb77e610694e64ca

            if (maxExclusive <= 0)
            {
                throw new ArgumentException("Max Exclusive must be positive");
            }

            if (maxExclusive == 1)
            {
                return 0;
            }

            int bits = (int)Math.Ceiling(Math.Log(maxExclusive, 2));
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < (uint)maxExclusive)
                {
                    return (int)result;
                }
            }
        }

        public override int Next(int minInclusive, int maxExclusive)
        {

            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentException("MaxExclusive must be larger than MinInclusive");
            }

            int range = (maxExclusive - minInclusive);
            return Next(range) + minInclusive;
        }

        public long NextInt64()
        {
            while (true)
            {
                // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
                // if the value is actually long.MaxValue, as the method is defined to return a value
                // in the range [0, long.MaxValue).
                ulong result = NextUInt64() >> 1;
                if (result != long.MaxValue)
                {
                    return (long)result;
                }
            }
        }
        public long NextInt64(long maxExclusive)
        {
            if (maxExclusive <= 0)
            {
                throw new ArgumentException("Max Exclusive must be positive");
            }

            if (maxExclusive == 1)
            {
                return 0;
            }

            // BitOperations : https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs
            // Narrow down to the smallest range [0, 2^bits] that contains maxValue.
            // Then repeatedly generate a value in that outer range until we get one within the inner range.
            int bits = (int)Math.Ceiling(Math.Log(maxExclusive, 2));
            while (true)
            {
                ulong result = NextUInt64() >> (sizeof(ulong) * 8 - bits);
                if (result < (ulong)maxExclusive)
                {
                    return (long)result;
                }
            }
        }

        public long NextInt64(long minInclusive, long maxExclusive)
        {

            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentException("MaxExclusive must be larger than MinInclusive");
            }

            long range = (maxExclusive - minInclusive);
            return NextInt64(range) + minInclusive;
        }

        public abstract ulong NextUInt64();
    }
}

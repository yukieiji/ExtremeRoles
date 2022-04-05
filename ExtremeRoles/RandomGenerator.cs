using System;
using System.Security.Cryptography;

namespace ExtremeRoles
{
    public static class RandomGenerator
    {
        public static PermutedCongruentialGenerator Instance;
        public static bool prevValue = false;

        public static void Initialize()
        {
            bool useStrongGen = OptionHolder.AllOption[
                (int)OptionHolder.CommonOptionKey.UseStrongRandomGen].GetValue();
            if (Instance == null)
            {
                createGlobalRandomGenerator(useStrongGen);
            }
            else
            {
                if (useStrongGen != prevValue)
                {
                    createGlobalRandomGenerator(useStrongGen);
                }
                Instance.Next();
            }
        }

        private static void createGlobalRandomGenerator(bool isStrong)
        {
            if (isStrong)
            {
                Instance = new PermutedCongruentialGenerator(
                    (ulong)createLongStrongSeed(),
                    (ulong)createLongStrongSeed());
                UnityEngine.Random.InitState(createStrongRandomSeed());
            }
            else
            {
                Instance = new PermutedCongruentialGenerator(
                    PermutedCongruentialGenerator.GuidBasedSeed());
                UnityEngine.Random.InitState(createNormalRandomSeed());
            }
            prevValue = isStrong;
        }

        public static Random GetTempGenerator()
        {
            bool useStrongGen = OptionHolder.AllOption[
                (int)OptionHolder.CommonOptionKey.UseStrongRandomGen].GetValue();

            if (useStrongGen)
            {
                return new Random(createStrongRandomSeed());
            }
            else
            {
                return new Random(createNormalRandomSeed());
            }
        }

        private static int createNormalRandomSeed()
        {
            return ((int)DateTime.Now.Ticks & 0x0000FFFF) + UnityEngine.SystemInfo.processorFrequency;
        }

        private static int createStrongRandomSeed()
        {
            var bs = new byte[4];
            //Int32と同じサイズのバイト配列にランダムな値を設定する
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bs);
            }
            //RNGCryptoServiceProviderで得たbit列をInt32型に変換してシード値とする。
            return BitConverter.ToInt32(bs, 0);
        }

        private static long createLongStrongSeed()
        {
            var bs = new byte[8];
            //Int64と同じサイズのバイト配列にランダムな値を設定する
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bs);
            }
            //RNGCryptoServiceProviderで得たbit列をInt32型に変換してシード値とする。
            return BitConverter.ToInt64(bs, 0);
        }

    }

    public class PermutedCongruentialGenerator
    {
        /*
            以下のURLの実装を元に実装
             https://github.com/igiagkiozis/PCGSharp
            
            ToDo:PCG32-XSH-RR => PCG32-RXS-M-XS
        
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
            ulong seed, ulong state = ShiftedIncrement)
        {
            initialize(seed, state);
        }

        public int Next()
        {
            uint result = NextUInt();
            return (int)(result >> 1);
        }

        public int Next(int maxExclusive)
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

        public int Next(int minInclusive, int maxExclusive)
        {

            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentException("MaxExclusive must be larger than MinInclusive");
            }

            int range = (maxExclusive - minInclusive);
            return Next(range) + minInclusive;
        }

        public uint NextUInt()
        {
            ulong oldState = this.state;
            this.state = unchecked(oldState * Multiplier + this.increment);
            uint xorShifted = (uint)(((oldState >> 18) ^ oldState) >> 27);
            int rot = (int)(oldState >> 59);
            uint result = (xorShifted >> rot) | (xorShifted << ((-rot) & 31));
            return result;
        }

        public void SetStream(ulong sequence)
        {
            this.increment = (sequence << 1) | 1;
        }

        public static ulong GuidBasedSeed()
        {
            ulong upper = (ulong)(Environment.TickCount ^ Guid.NewGuid().GetHashCode()) << 32;
            ulong lower = (ulong)(Environment.TickCount ^ Guid.NewGuid().GetHashCode());
            return (upper | lower);
        }

        private void initialize(
            ulong seed, ulong initStete)
        {
            this.state = 0ul;
            SetStream(initStete);

            NextUInt();

            this.state += seed;

            NextUInt();

        }

    }
}

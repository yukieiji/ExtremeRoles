using System;
using System.Security.Cryptography;

using ExtremeRoles.Module.RNG;

namespace ExtremeRoles
{
    public static class RandomGenerator
    {
        public static RNGBase Instance;
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
                    createLongStrongSeed(),
                    createLongStrongSeed());
                UnityEngine.Random.InitState(createStrongRandomSeed());
            }
            else
            {
                Instance = new Pcg64XshRr(guidBasedSeed());
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

            Helper.Logging.Debug($"Int32 SeedValue:{string.Join("", bs)}");

            //RNGCryptoServiceProviderで得たbit列をInt32型に変換してシード値とする。
            return BitConverter.ToInt32(bs, 0);
        }

        public static ulong CreateLongStrongSeed()
        {
            var bs = new byte[8];
            //Int64と同じサイズのバイト配列にランダムな値を設定する
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bs);
            }

            Helper.Logging.Debug($"UInt64 Seed:{string.Join("", bs)}");

            //RNGCryptoServiceProviderで得たbit列をUInt64型に変換してシード値とする。
            return BitConverter.ToUInt64(bs, 0);
        }
        private static ulong guidBasedSeed()
        {
            ulong upper = (ulong)(Environment.TickCount ^ Guid.NewGuid().GetHashCode()) << 32;
            ulong lower = (ulong)(Environment.TickCount ^ Guid.NewGuid().GetHashCode());
            return (upper | lower);
        }

    }
}

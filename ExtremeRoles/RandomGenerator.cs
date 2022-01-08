using System;
using System.Security.Cryptography;

namespace ExtremeRoles
{
    public static class RandomGenerator
    {
        public static Random Instance; 

        public static void Initialize()
        {
            bool useStrongGen = OptionHolder.AllOption[
                (int)OptionHolder.CommonOptionKey.UseStrongRandomGen].GetValue();
            if (useStrongGen)
            {
                Instance = new Random(createStrongRandomSeed());
                UnityEngine.Random.InitState(createStrongRandomSeed());
            }
            else
            {
                Instance = new Random(createNormalRandomSeed());
                UnityEngine.Random.InitState(createNormalRandomSeed());
            }

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

    }
}

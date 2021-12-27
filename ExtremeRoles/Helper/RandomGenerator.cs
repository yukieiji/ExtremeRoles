using System;
using System.Security.Cryptography;

namespace ExtremeRoles.Helper
{
    public static class RandomGenerator
    {
        public static Random CreateStrong()
        {
            return new Random(createStrongRandomSeed());
        }

        public static void SetUnityStrongRandomSeed()
        {
            UnityEngine.Random.InitState(createStrongRandomSeed());
        }

        public static Random Create()
        {
            return new Random(createNormalRandomSeed());
        }

        public static void SetUnityRandomSeed()
        {
            UnityEngine.Random.InitState(createNormalRandomSeed());
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

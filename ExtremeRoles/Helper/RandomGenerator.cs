using System;
using System.Security.Cryptography;

namespace ExtremeRoles.Helper
{
    public static class RandomGenerator
    {
        public static Random Create()
        {
            return new Random(createRandomSeed());
        }

        public static void SetUnityRandomSeed()
        {
            UnityEngine.Random.InitState(createRandomSeed());
        }

        private static int createRandomSeed()
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

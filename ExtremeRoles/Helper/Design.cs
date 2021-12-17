using UnityEngine;

namespace ExtremeRoles.Helper
{
    public class Design
    {
        public static string ConcatString(string baseString, string addString)
        {
            return string.Format(
                "{0}{1}",
                baseString,
                addString);
        }

        public static string Cs(Color c, string s)
        {
            return string.Format(
                "<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>",
                ToByte(c.r),
                ToByte(c.g),
                ToByte(c.b),
                ToByte(c.a), s);
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}

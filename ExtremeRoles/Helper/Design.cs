using UnityEngine;

using System.Text.RegularExpressions;

namespace ExtremeRoles.Helper
{
    public static class Design
    {
        public static string ColoedString(Color c, string s)
        {
            return string.Format(
                "<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>",
                toByte(c.r),
                toByte(c.g),
                toByte(c.b),
                toByte(c.a), s);
        }

        public static string CleanPlaceHolder(string value)
        {
            return Regex.Replace(
                value, "\\{[0-9]+\\}",
                Translation.GetString("gameReplace"));
        }


        private static byte toByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }
}

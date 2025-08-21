using System.Text.RegularExpressions;

using UnityEngine;

namespace ExtremeRoles.Helper;

public static class Design
{
	private const byte colorMask = 0xFF;

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
            Tr.GetString("gameReplace"));
    }


    private static byte toByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }


	public static Color32 ToRGBA(uint value)
	{
		return new Color32(
			(byte)((value >> 24) & colorMask),
			(byte)((value >> 16) & colorMask),
			(byte)((value >> 8) & colorMask),
			(byte)(value & colorMask)
		);
	}

	public static uint FromRGBA(Color32 color)
	{
		uint newColor = 0;
		newColor |= (uint)color.r << 24;
		newColor |= (uint)color.g << 16;
		newColor |= (uint)color.b << 8;
		newColor |= (uint)color.a;
		return newColor;
	}
}

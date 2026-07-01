using System.Text.RegularExpressions;

using UnityEngine;

namespace ExtremeRoles.Helper;

public static class Design
{
	private const byte colorMask = 0xFF;

	public static string ColoredString(Color c, string s)
    {
		string colorCode = ColorUtility.ToHtmlStringRGBA(c);
        return $"<color=#{colorCode}>{s}</color>";
    }

    public static string CleanPlaceHolder(string value)
    {
        return Regex.Replace(
            value, "\\{[0-9]+\\}",
            Tr.GetString("gameReplace"));
    }

	/// <summary>
	///  Converts 32-bit uint to Color32.
	/// The format is 0x|RR|GG|BB|AA|, store 8 bits as a single value
	/// </summary>
	/// <param name="value">32-bit unsigned integer color</param>
	public static Color32 ToRGBA(uint value)
	{
		return new Color32(
			(byte)((value >> 24) & colorMask),
			(byte)((value >> 16) & colorMask),
			(byte)((value >> 8) & colorMask),
			(byte)(value & colorMask)
		);
	}

	/// <summary>
	/// Converts Color32 to 32-bit uint.
	/// The format is 0x|RR|GG|BB|AA|, store 8 bits as a single value
	/// </summary>
	/// <param name="color">Unity color32</param>
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

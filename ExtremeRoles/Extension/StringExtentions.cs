namespace ExtremeRoles.Extension.Strings;

public static class StringExtentions
{
	public static int CountLine(this string str)
	{
		int count = 1;
		foreach (char c in str)
		{
			if (c == '\n')
			{
				++count;
			}
		}
		return count;
	}
}

using System.Text;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.Interface;

public interface IInfoOverlayPanelModel
{
	public void UpdateVisual();
	public (string, string) GetInfoText();

	protected static void AddHudStringWithChildren(StringBuilder builder, IOption option, int indent = -1)
	{
		if (option.IsViewActive)
		{
			builder.AppendLine(toHudString(option));
		}
		addChildrenOptionHudString(builder, option, indent + 1);
	}

	protected static string ToHudStringWithChildren(IOption option, int indent = 0)
	{
		var builder = new StringBuilder();
		AddHudStringWithChildren(builder, option, indent);
		return builder.ToString();
	}

	private static void addChildrenOptionHudString(
		in StringBuilder builder,
		IOption parentOption,
		int prefixIndentCount)
	{
		if (!OptionManager.Instance.TryGetChild(parentOption, out var child))
		{
			return;
		}
		string indent = new string(' ', prefixIndentCount * 4);
		foreach (var option in child)
		{
			if (option.IsViewActive)
			{
				builder.Append(indent);

				string text = toHudString(option)
					.TrimEnd('\r', '\n')
					.Replace("\r\n", "\n")
					.Replace("\n", $"\n{indent}");
				builder.AppendLine(text);
			}

			addChildrenOptionHudString(in builder, option, prefixIndentCount + 1);
		}
	}

	private static string toHudString(in IOption option)
		=> $"・{option.TransedTitle}: {option.TransedValue}";
}

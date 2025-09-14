using System.Text;

using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.Interface;

public interface IInfoOverlayPanelModel
{
	public (string, string) GetInfoText();

	protected static string ToHudStringWithChildren(IOldOption option, int indent = 0)
	{
		var builder = new StringBuilder();
		if (!option.Info.IsHidden && option.IsActiveAndEnable)
		{
			builder.AppendLine(toHudString(option));
		}

		addChildrenOptionHudString(builder, option, indent + 1);
		return builder.ToString();
	}

	private static void addChildrenOptionHudString(
		in StringBuilder builder,
		IOldOption parentOption,
		int prefixIndentCount)
	{
		foreach (var child in parentOption.Relation.Children)
		{
			if (!child.Info.IsHidden && child.IsActiveAndEnable)
			{
				builder.Append(' ', prefixIndentCount * 4);
				builder.AppendLine(toHudString(child));
			}

			addChildrenOptionHudString(in builder, child, prefixIndentCount + 1);
		}
	}

	private static string toHudString(in IOldOption option)
		=> $"{option.Title}: {option.ValueString}";
}

using System.Collections.Generic;

namespace ExtremeRoles.Module.NewOption.OLDS.Implemented;

public sealed class IntCustomOption : CustomOptionBase<int, int>
{
	private int maxValue;
	private int minValue;

	public IntCustomOption(
		int id,
		string name,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo enableCheckOption = null,
		OptionTab tab = OptionTab.General) : base(
			id, name,
			createSelection(min, max, step).ToArray(),
			defaultValue, parent,
			isHeader, isHidden,
			format, invert,
			enableCheckOption, tab)
	{
		minValue = Option[0];
		maxValue = Option[ValueCount - 1];
	}

	public override int GetValue() => Option[CurSelection];

	public override void Update(int newValue)
	{
		int newMaxValue = maxValue / newValue;

		List<int> newSelections = new List<int>();
		for (int s = minValue; s <= newMaxValue; ++s)
		{
			newSelections.Add(s);
		}

		Option = newSelections.ToArray();
		UpdateSelection(CurSelection);
	}

	private static List<int> createSelection(int min, int max, int step)
	{
		List<int> selection = new List<int>();
		for (int s = min; s <= max; s += step)
		{
			selection.Add(s);
		}

		return selection;
	}

}

public sealed class IntDynamicCustomOption : CustomOptionBase<int, int>
{
	private int step;
	public IntDynamicCustomOption(
		int id, string name,
		int defaultValue,
		int min, int step,
		IOptionInfo parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo enableCheckOption = null,
		OptionTab tab = OptionTab.General,
		int tempMaxValue = 0) : base(
			id, name,
			createSelection(
				min, step, defaultValue, tempMaxValue).ToArray(),
			defaultValue, parent,
			isHeader, isHidden,
			format, invert,
			enableCheckOption, tab)
	{
		this.step = step;
	}

	public override int GetValue() => Option[CurSelection];

	public override void Update(int newValue)
	{
		int minValue = Option[0];

		List<int> newSelections = new List<int>();
		for (int s = minValue; s <= newValue; s += step)
		{
			newSelections.Add(s);
		}
		Option = newSelections.ToArray();
		UpdateSelection(CurSelection);
	}

	private static List<int> createSelection(
		int min, int step, int defaultValue, int tempMaxValue)
	{
		List<int> selection = new List<int>();

		if (tempMaxValue == 0)
		{
			tempMaxValue = min + step < defaultValue ? defaultValue : min + step;
		}

		for (int s = min; s <= tempMaxValue; s += step)
		{
			selection.Add(s);
		}

		return selection;
	}
}

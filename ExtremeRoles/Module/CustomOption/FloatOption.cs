using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption;

public sealed class FloatCustomOption : CustomOptionBase<float, float>
{
    public FloatCustomOption(
        int id, string name,
        float defaultValue,
        float min, float max, float step,
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
    { }

    public override float GetValue() => this.Option[CurSelection];

    private static List<float> createSelection(float min, float max, float step)
    {

        List<float> selection = new List<float>();

        decimal dStep = new decimal(step);
        decimal dMin = new decimal(min);
        decimal dMax = new decimal(max);

        for (decimal s = dMin; s <= dMax; s += dStep)
        {
            selection.Add(((float)(decimal.ToDouble(s))));
        }

        return selection;
    }
}

public sealed class FloatDynamicCustomOption : CustomOptionBase<float, float>
{
    private float step;
    public FloatDynamicCustomOption(
        int id, string name,
        float defaultValue,
        float min, float step,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        OptionTab tab = OptionTab.General,
        float tempMaxValue = 0.0f) : base(
            id, name,
            createSelection(min, step, defaultValue, tempMaxValue).ToArray(),
            defaultValue, parent,
            isHeader, isHidden,
            format, invert,
            enableCheckOption, tab)
    {
        this.step = step;
    }

    public override float GetValue() => this.Option[CurSelection];

    public override void Update(float newValue)
    {
        decimal dStep = new decimal(this.step);
        decimal dMin = new decimal(this.Option[0]);
        decimal dMax = new decimal(newValue);

        List<float> newSelection = new List<float>();
        for (decimal s = dMin; s <= dMax; s += dStep)
        {
            newSelection.Add(((float)(decimal.ToDouble(s))));
        }
        this.Option = newSelection.ToArray();
        this.UpdateSelection(this.CurSelection);
    }

    private static List<float> createSelection(
        float min, float step, float defaultValue, float floatTempMaxValue)
    {

        List<float> selection = new List<float>();

        decimal dStep = new decimal(step);
        decimal dMin = new decimal(min);

        decimal tempMaxValue;
        if (floatTempMaxValue == 0.0f)
        {
            tempMaxValue = (min + step) < defaultValue ? new decimal(defaultValue) : dMin + dStep;
        }
        else
        {
            tempMaxValue = new decimal(floatTempMaxValue);
        }

        for (decimal s = dMin; s <= tempMaxValue; s += dStep)
        {
            selection.Add(((float)(decimal.ToDouble(s))));
        }

        return selection;
    }
}

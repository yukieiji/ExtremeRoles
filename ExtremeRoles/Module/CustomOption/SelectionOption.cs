using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.CustomOption;

public sealed class SelectionCustomOption : CustomOptionBase<int, string>
{
    public SelectionCustomOption(
        int id, string name,
        string[] selections,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        OptionTab tab = OptionTab.General) : base(
            id, name, selections, "",
            parent, isHeader, isHidden,
            format, invert, enableCheckOption, tab)
    { }

    public SelectionCustomOption(
        int id, string name,
        string[] selections,
        int defaultIndex,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null) : base(
            id, name, selections, selections[defaultIndex],
            parent, isHeader, isHidden,
            format, invert, enableCheckOption)
    { }

    public SelectionCustomOption(
        int id, string name,
        Type selectionType,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null) : this(
            id, name, getEnumString(selectionType).ToArray(),
            parent, isHeader, isHidden,
            format, invert, enableCheckOption)
    { }

    public override int GetValue() => CurSelection;

    private static List<string> getEnumString(Type enumType)
    {
        var list = new List<string>();
        foreach (object enumValue in Enum.GetValues(enumType))
        {
            list.Add(enumValue.ToString());
        }
        return list;
    }
}

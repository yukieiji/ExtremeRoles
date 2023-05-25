namespace ExtremeRoles.Module.CustomOption;

public sealed class BoolCustomOption : CustomOptionBase<bool, string>
{
    public BoolCustomOption(
        int id, string name,
        bool defaultValue,
        IOptionInfo parent = null,
        bool isHeader = false,
        bool isHidden = false,
        OptionUnit format = OptionUnit.None,
        bool invert = false,
        IOptionInfo enableCheckOption = null,
        OptionTab tab = OptionTab.General) : base(
            id, name,
            new string[] { "optionOff", "optionOn" },
            defaultValue ? "optionOn" : "optionOff",
            parent, isHeader, isHidden,
            format, invert,
            enableCheckOption, tab)
    { }

    public override bool GetValue() => CurSelection > 0;
}

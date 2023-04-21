using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomOption;

public enum OptionTab : byte
{
    General,
    Crewmate,
    Impostor,
    Neutral,
    Combination,
    GhostCrewmate,
    GhostImpostor,
    GhostNeutral,
}

public enum OptionUnit : byte
{
    None,
    Preset,
    Second,
    Minute,
    Shot,
    Multiplier,
    Percentage,
    ScrewNum,
    VoteNum
}

public interface IOptionInfo
{
    public int CurSelection { get; }
    public bool Enabled { get; }
    public int Id { get; }
    public string Name { get; }
    public bool IsHidden { get; }
    public bool IsHeader { get; }
    public int ValueCount { get; }
    public OptionTab Tab { get; }
    public IOptionInfo Parent { get; }
    public List<IOptionInfo> Children { get; }
    public OptionBehaviour Body { get; }

    public bool IsActive();
    public void SetHeaderTo(bool enable);
    public void SetOptionBehaviour(OptionBehaviour newBehaviour);
    public string GetTranslatedValue();
    public string GetTranslatedName();
    public void UpdateSelection(int newSelection);
    public void SaveConfigValue();
    public void SwitchPreset();
    public string ToHudString();
    public string ToHudStringWithChildren(int indent = 0);
}

public interface IValueOption<Value> : IOptionInfo
{
    public Value GetValue();
    public void Update(Value newValue);
    public void SetUpdateOption(IValueOption<Value> option);
}

public abstract class CustomOptionBase<OutType, SelectionType> : IValueOption<OutType>
{

    public int CurSelection { get; private set; }
    public bool IsHidden { get; private set; }
    public bool IsHeader { get; private set; }
    public List<IOptionInfo> Children { get; private set; }
    public OptionBehaviour Body { get; private set; }

    public OptionTab Tab { get; init; }
    public IOptionInfo Parent { get; init; }
    public int Id { get; init; }
    public string Name { get; init; }

    public int ValueCount => this.Option.Length;
    public bool Enabled
        => this.CurSelection != this.defaultSelection;

    protected SelectionType[] Option = new SelectionType[1];

    private bool enableInvert = false;
    private int defaultSelection = 0;

    private ConfigEntry<int> entry = null;
    private List<IValueOption<OutType>> withUpdateOption = new List<IValueOption<OutType>>();
    private IOptionInfo forceEnableCheckOption = null;
    private OptionUnit format = OptionUnit.None;
    
    private const string IndentStr = "    ";

    public CustomOptionBase(
        int id,
        string name,
        SelectionType[] selections,
        SelectionType defaultValue,
        IOptionInfo parent,
        bool isHeader,
        bool isHidden,
        OptionUnit format,
        bool invert,
        IOptionInfo enableCheckOption,
        OptionTab tab = OptionTab.General)
    {

        this.Tab = tab;
        this.Parent = parent;

        int index = Array.IndexOf(selections, defaultValue);

        this.Id = id;
        this.Name = name;

        this.format = format;
        this.defaultSelection = index;

        this.IsHeader = isHeader;
        this.IsHidden = isHidden;

        this.Children = new List<IOptionInfo>();
        this.withUpdateOption.Clear();
        this.forceEnableCheckOption = enableCheckOption;

        if (parent != null)
        {
            this.enableInvert = invert;
            parent.Children.Add(this);
        }

        this.CurSelection = 0;
        if (id > 0)
        {
            bindConfig();
            this.CurSelection = Mathf.Clamp(this.entry.Value, 0, selections.Length - 1);
        }

        Logging.Debug($"OptinId:{this.Id}    Name:{this.Name}");

        AllOptionHolder.Instance.Add(this.Id, this);
    }

    public void AddToggleOptionCheckHook(StringNames targetOption)
    {
        Patches.Option.GameOptionsMenuStartPatch.AddHook(
            targetOption, x => this.IsHidden = !x.GetBool());
    }

    public virtual void Update(OutType newValue)
    {
        return;
    }

    public string GetTranslatedName() => Translation.GetString(this.Name);

    public string GetTranslatedValue()
    {
        string sel = this.Option[this.CurSelection].ToString();
        if (this.format != OptionUnit.None)
        {
            return string.Format(
                Translation.GetString(
                    this.format.ToString()), sel);
        }
        return Translation.GetString(sel);
    }

    public bool IsActive()
    {
        if (this.IsHidden)
        {
            return false;
        }

        if (this.IsHeader || this.Parent == null)
        {
            return true;
        }

        IOptionInfo parent = this.Parent;
        bool active = true;

        while (parent != null && active)
        {
            active = parent.Enabled;
            parent = parent.Parent;
        }

        if (this.enableInvert)
        {
            active = !active;
        }

        if (this.forceEnableCheckOption is not null)
        {
            bool forceEnable = this.forceEnableCheckOption.Enabled;

            if (this.forceEnableCheckOption.Parent is not null)
            {
                forceEnable = forceEnable && this.forceEnableCheckOption.Parent.IsActive();
            }

            active = active && forceEnable;
        }
        return active;
    }

    public void SetUpdateOption(IValueOption<OutType> option)
    {
        this.withUpdateOption.Add(option);
        option.Update(this.GetValue());
    }

    public void UpdateSelection(int newSelection)
    {
        int length = this.ValueCount;

        this.CurSelection = Mathf.Clamp(
            (newSelection + length) % length,
            0, length - 1);

        if (this.Body is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = this.CurSelection;
            stringOption.ValueText.text = this.GetTranslatedValue();
        }

        foreach (IValueOption<OutType> option in this.withUpdateOption)
        {
            option.Update(this.GetValue());
        }

        if (AmongUsClient.Instance &&
            AmongUsClient.Instance.AmHost &&
            CachedPlayerControl.LocalPlayer)
        {
            if (this.Id == 0)
            {
                AllOptionHolder.Instance.SwitchPreset(this.CurSelection); // Switch presets
            }
            else if (this.entry != null)
            {
                this.entry.Value = this.CurSelection; // Save selection to config
            }
            AllOptionHolder.Instance.ShareOptionSelections();// Share all selections
        }
    }

    public void SaveConfigValue()
    {
        if (this.entry != null)
        {
            this.entry.Value = this.CurSelection;
        }
    }

    public void SwitchPreset()
    {
        bindConfig();
        this.UpdateSelection(Mathf.Clamp(
            this.entry.Value, 0,
            this.ValueCount - 1));
    }

    public void SetHeaderTo(bool enable)
    {
        this.IsHeader = enable;
    }

    public void SetOptionBehaviour(OptionBehaviour newBehaviour)
    {
        this.Body = newBehaviour;
    }

    public void SetOptionUnit(OptionUnit unit)
    {
        this.format = unit;
    }

    public string ToHudString() =>
        this.IsActive() ? $"{this.GetTranslatedName()}: {this.GetTranslatedValue()}" : string.Empty;

    public string ToHudStringWithChildren(int indent = 0)
    {
        StringBuilder builder = new StringBuilder();
        string optStr = this.ToHudString();
        if (!this.IsHidden && optStr != string.Empty)
        {
            builder.AppendLine(optStr);
        }
        addChildrenOptionHudString(ref builder, this, indent);
        return builder.ToString();
    }

    public abstract OutType GetValue();

    private void bindConfig()
    {
        this.entry = ExtremeRolesPlugin.Instance.Config.Bind(
            AllOptionHolder.Instance.ConfigPreset,
            this.cleanName(),
            this.defaultSelection);
    }

    private string cleanName()
    {
        string nameClean = Regex.Replace(this.Name, "<.*?>", "");
        nameClean = Regex.Replace(nameClean, "^-\\s*", "");
        return nameClean.Trim();
    }

    private static void addChildrenOptionHudString(
        ref StringBuilder builder,
        IOptionInfo parentOption,
        int prefixIndentCount)
    {
        string prefixIndent = prefixIndentCount != 0 ?
            string.Concat(Enumerable.Repeat(IndentStr, prefixIndentCount)) :
            string.Empty;

        foreach (var child in parentOption.Children)
        {
            string childOptionStr = child.ToHudString();

            if (childOptionStr != string.Empty)
            {
                builder.AppendLine(
                    string.Concat(prefixIndent, childOptionStr));
            }

            addChildrenOptionHudString(ref builder, child, prefixIndentCount + 1);
        }
    }
}

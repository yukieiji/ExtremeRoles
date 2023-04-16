using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

using BepInEx.Configuration;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module;

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

    public interface IOption
    {
        public int CurSelection { get; }
        public bool Enabled { get; }
        public int Id { get; }
        public string Name { get; }
        public bool IsHidden { get; }
        public bool IsHeader { get; }
        public int ValueCount { get; }
        public OptionTab Tab { get; }
        public IOption Parent { get; }

    // インターフェースとして要らなくなった奴ら
    // TODO:こいつらも消す
    public List<IOption> Children { get; }
    public OptionBehaviour Body { get; }
    public IOption ForceEnableCheckOption { get; }
    public int DefaultSelection { get; }

    public bool IsActive();
    public void SetHeaderTo(bool enable);
    public void SetOptionBehaviour(OptionBehaviour newBehaviour);
    public string GetTranslatedValue();
    public string GetTranslatedName();
    public void UpdateSelection(int newSelection);
    public void SaveConfigValue();
    public void SwitchPreset();
    public string ToHudString();
    public string ToHudStringWithChildren(int indent=0);

    public dynamic GetValue();
}

public interface IWithUpdatableOption<T>
{
    public void Update(T newValue);
    public void SetUpdateOption(IWithUpdatableOption<T> option);
}


public abstract class CustomOptionBase<OutType, SelectionType> : IOption, IWithUpdatableOption<OutType>
{
    public int CurSelection => this.curSelection;
    public int DefaultSelection => this.defaultSelection;
    public int Id => this.id;
    public string Name => this.name;
    
    public bool IsHidden => this.isHidden;
    public bool IsHeader => this.isHeader;

    public int ValueCount => this.Selections.Length;

    public OptionTab Tab { get; }
    public IOption Parent { get; }
    public IOption ForceEnableCheckOption => this.forceEnableCheckOption;
    public List<IOption> Children => this.children;

    public OptionBehaviour Body
    {
        get => this.behaviour;
    }
    public bool Enabled
    {
        get
        {
            return this.CurSelection != this.defaultSelection;
        }
    }
    
    protected SelectionType[] Selections;

    private OptionUnit stringFormat;
    private List<IWithUpdatableOption<OutType>> withUpdateOption = new List<IWithUpdatableOption<OutType>>();

    private IOption forceEnableCheckOption;
    private List<IOption> children = new List<IOption>();

    private bool enableInvert;
    private bool isHidden;
    private bool isHeader;
    private string name;
    private int curSelection = 0;
    private int defaultSelection = 0;
    private int id = 0;
    private ConfigEntry<int> entry;

    private OptionBehaviour behaviour;

    private const string IndentStr = "    ";

    public CustomOptionBase(
        int id,
        string name,
        SelectionType[] selections,
        SelectionType defaultValue,
        IOption parent,
        bool isHeader,
        bool isHidden,
        OptionUnit format,
        bool invert,
        IOption enableCheckOption,
        OptionTab tab = OptionTab.General)
    {

        this.Tab = tab;
        this.Parent = parent;

        int index = Array.IndexOf(selections, defaultValue);

        this.id = id;
        this.name = name;
        this.stringFormat = format;
        this.Selections = selections;
        this.defaultSelection = index >= 0 ? index : 0;

        this.isHeader = isHeader;
        this.isHidden = isHidden;
        this.enableInvert = false;

        this.children.Clear();
        this.withUpdateOption.Clear();
        this.forceEnableCheckOption = enableCheckOption;

        if (parent != null)
        {
            this.enableInvert = invert;
            parent.Children.Add(this);
        }

        this.curSelection = 0;
        if (id > 0)
        {
            bindConfig();
            this.curSelection = Mathf.Clamp(this.entry.Value, 0, selections.Length - 1);
        }

        Logging.Debug($"OptinId:{this.Id}    Name:{this.name}");

        OptionHolder.AllOption.Add(this.Id, this);
    }

    public void AddToggleOptionCheckHook(StringNames targetOption)
    {
        Patches.Option.GameOptionsMenuStartPatch.AddHook(
            targetOption, x => this.isHidden = !x.GetBool());
    }

    public virtual void Update(OutType newValue)
    {
        return;
    }

    public string GetTranslatedName() => Translation.GetString(this.name);

    public string GetTranslatedValue()
    {
        string sel = this.Selections[this.curSelection].ToString();
        if (this.stringFormat != OptionUnit.None)
        {
            return string.Format(
                Translation.GetString(
                    this.stringFormat.ToString()), sel);
        }
        return Translation.GetString(sel);
    }

    public bool IsActive()
    {
        if (this.isHidden)
        {
            return false;
        }

        if (this.isHeader || this.Parent == null)
        {
            return true;
        }

        IOption parent = this.Parent;
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

        if (this.forceEnableCheckOption != null)
        {
            bool forceEnable = this.forceEnableCheckOption.Enabled;

            if (this.forceEnableCheckOption.Parent != null)
            {
                forceEnable = forceEnable && this.forceEnableCheckOption.Parent.IsActive();
            }

            active = active && forceEnable;
        }

        return active;
    }

    public void SetUpdateOption(IWithUpdatableOption<OutType> option)
    {
        this.withUpdateOption.Add(option);
        option.Update(this.GetValue());
    }

    public void UpdateSelection(int newSelection)
    {
        int length = this.Selections.Length;

        this.curSelection = Mathf.Clamp(
            (newSelection + length) % length,
            0, length - 1);

        if (this.behaviour is StringOption stringOption)
        {
            stringOption.oldValue = stringOption.Value = this.curSelection;
            stringOption.ValueText.text = this.GetTranslatedValue();
        }

        if (this.withUpdateOption.Count != 0)
        {
            foreach (IWithUpdatableOption<OutType> option in this.withUpdateOption)
            {
                option.Update(this.GetValue());
            }
        }

        if (AmongUsClient.Instance &&
            AmongUsClient.Instance.AmHost && 
            CachedPlayerControl.LocalPlayer)
        {
            if (this.id == 0)
            {
                OptionHolder.SwitchPreset(this.curSelection); // Switch presets
            }
            else if (this.entry != null)
            {
                this.entry.Value = this.curSelection; // Save selection to config
            }
            OptionHolder.ShareOptionSelections();// Share all selections
        }
    }

    public void SaveConfigValue()
    {
        if (this.entry != null)
        {
            this.entry.Value = this.curSelection;
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
        this.isHeader = enable;
    }

    public void SetOptionBehaviour(OptionBehaviour newBehaviour)
    {
        this.behaviour = newBehaviour;
    }

    public void SetOptionUnit(OptionUnit unit)
    {
        this.stringFormat = unit;
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

    private void bindConfig()
    {
        this.entry = ExtremeRolesPlugin.Instance.Config.Bind(
            OptionHolder.ConfigPreset,
            this.cleanName(),
            this.defaultSelection);
    }

    private string cleanName()
    {
        string nameClean = Regex.Replace(this.name, "<.*?>", "");
        nameClean = Regex.Replace(nameClean, "^-\\s*", "");
        return nameClean.Trim();
    }

    private static void addChildrenOptionHudString(
        ref StringBuilder builder,
        IOption parentOption,
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
        public abstract dynamic GetValue();
    }
}

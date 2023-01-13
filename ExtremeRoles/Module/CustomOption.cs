using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UnityEngine;

using BepInEx.Configuration;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module
{
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
        Second,
        Minute,
        Shot,
        Multiplier,
        Percentage,
        ScrewNum,
        VoteNum
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
        public void SetOptionBehaviour(OptionBehaviour newBehaviour);
        public string GetTranslatedValue();
        public string GetTranslatedName();
        public void UpdateSelection(int newSelection);
        public void SaveConfigValue();
        public void SwitchPreset();
        public string ToHudString();
        public string ToHudStringWithChildren(int indent=0);

        // This is HotFix for HideNSeek
        public dynamic GetDefault();
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

        public OptionTab Tab => this.tab;
        public IOption Parent => this.parent;
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

        private OptionTab tab;
        private IOption parent;
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

            this.tab = tab;

            int index = Array.IndexOf(selections, defaultValue);

            this.id = id;
            this.name = name;
            this.stringFormat = format;
            this.Selections = selections;
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
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

            if (this.isHeader || this.parent == null)
            {
                return true;
            }

            IOption parent = this.parent;
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

            if (AmongUsClient.Instance?.AmHost == true && CachedPlayerControl.LocalPlayer)
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

        public abstract dynamic GetDefault();
        public abstract dynamic GetValue();
    }

    public sealed class BoolCustomOption : CustomOptionBase<bool, string>
    {
        public BoolCustomOption(
            int id, string name,
            bool defaultValue,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            OptionTab tab = OptionTab.General) : base(
                id, name,
                new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff",
                parent, isHeader, isHidden,
                format, invert,
                enableCheckOption, tab)
        { }

        public override dynamic GetDefault() => this.DefaultSelection > 0;
        public override dynamic GetValue() => CurSelection > 0;
    }

    public sealed class FloatCustomOption : CustomOptionBase<float, float>
    {
        public FloatCustomOption(
            int id, string name,
            float defaultValue,
            float min, float max, float step,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            OptionTab tab = OptionTab.General) : base(
                id, name,
                createSelection(min, max, step).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption, tab)
        { }

        public override dynamic GetDefault() => Selections[this.DefaultSelection];
        public override dynamic GetValue() => Selections[CurSelection];

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

    public sealed class IntCustomOption : CustomOptionBase<int, int>
    {
        private int maxValue;
        private int minValue;

        public IntCustomOption(
            int id,
            string name,
            int defaultValue,
            int min, int max, int step,
            IOption parent = null,
            bool isHeader = false, 
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            OptionTab tab = OptionTab.General) : base(
                id, name,
                createSelection(min, max, step).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption, tab)
        {
            this.minValue = this.Selections[0];
            this.maxValue = this.Selections[this.ValueCount - 1];
        }

        public override dynamic GetDefault() => Selections[this.DefaultSelection];
        public override dynamic GetValue() => Selections[CurSelection];

        public override void Update(int newValue)
        {
            int newMaxValue = this.maxValue / newValue;

            List<int> newSelections = new List<int>();
            for (int s = minValue; s <= newMaxValue; ++s)
            {
                newSelections.Add(s);
            }

            this.Selections = newSelections.ToArray();
            this.UpdateSelection(this.CurSelection);
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
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
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

        public override dynamic GetDefault() => Selections[this.DefaultSelection];
        public override dynamic GetValue() => Selections[CurSelection];

        public override void Update(int newValue)
        {
            int minValue = this.Selections[0];

            List<int> newSelections = new List<int>();
            for (int s = minValue; s <= newValue; s += this.step)
            {
                newSelections.Add(s);
            }
            this.Selections = newSelections.ToArray();
            this.UpdateSelection(this.CurSelection);
        }

        private static List<int> createSelection(
            int min, int step, int defaultValue, int tempMaxValue)
        {
            List<int> selection = new List<int>();

            if (tempMaxValue == 0)
            {
                tempMaxValue = (min + step) < defaultValue ? defaultValue : min + step;
            }

            for (int s = min; s <= tempMaxValue; s += step)
            {
                selection.Add(s);
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
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
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

        public override dynamic GetDefault() => Selections[this.DefaultSelection];
        public override dynamic GetValue() => Selections[CurSelection];

        public override void Update(float newValue)
        {
            decimal dStep = new decimal(this.step);
            decimal dMin = new decimal(this.Selections[0]);
            decimal dMax = new decimal(newValue);

            List<float> newSelection = new List<float>();
            for (decimal s = dMin; s <= dMax; s += dStep)
            {
                newSelection.Add(((float)(decimal.ToDouble(s))));
            }
            this.Selections = newSelection.ToArray();
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



    public sealed class SelectionCustomOption : CustomOptionBase<int, string>
    {
        public SelectionCustomOption(
            int id, string name,
            string[] selections,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null,
            OptionTab tab = OptionTab.General) : base(
                id, name, selections, "",
                parent, isHeader, isHidden,
                format, invert, enableCheckOption, tab)
        { }

        public SelectionCustomOption(
            int id, string name,
            string[] selections,
            int defaultIndex,
            IOption parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            IOption enableCheckOption = null) : base(
                id, name, selections, selections[defaultIndex],
                parent, isHeader, isHidden,
                format, invert, enableCheckOption)
        { }

        public override dynamic GetDefault() => this.DefaultSelection;
        public override dynamic GetValue() => CurSelection;
    }
}

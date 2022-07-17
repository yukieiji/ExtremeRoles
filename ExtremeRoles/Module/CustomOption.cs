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
        public string CleanedName { get; }
        public int DefaultSelection { get; }
        public bool Enabled { get; }
        public int Id { get; }
        public bool IsHidden { get; }
        public bool IsHeader { get; }
        public string Name { get; }
        public int ValueCount { get; }
        public IOption Parent { get; }
        public IOption ForceEnableCheckOption { get; }
        public List<IOption> Children { get; }
        public ConfigEntry<int> Entry { set; get; }

        public OptionBehaviour Body { set; get; }

        public bool IsActive();
        public string GetString();
        public string GetName();
        public void UpdateSelection(int newSelection);
        public void SaveConfigValue();
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
        
        public bool IsHidden => this.isHidden;
        public bool IsHeader => this.isHeader;

        public string Name => this.name;

        public int ValueCount => this.Selections.Length;

        public IOption Parent => this.parent;
        public IOption ForceEnableCheckOption => this.forceEnableCheckOption;
        public List<IOption> Children => this.children;

        public OptionBehaviour Body
        {
            get => this.behaviour;
            set
            {
                this.behaviour = value;
            }
        }

        public ConfigEntry<int> Entry
        {
            get => this.entry;
            set
            {
                this.entry = value;
            }
        }
        public virtual bool Enabled
        {
            get
            {
                return this.CurSelection != this.DefaultSelection;
            }
        }
        public string CleanedName
        {
            get
            {
                string nameClean = Regex.Replace(this.Name, "<.*?>", "");
                nameClean = Regex.Replace(nameClean, "^-\\s*", "");
                return nameClean.Trim();
            }
        }
        
        protected SelectionType[] Selections;

        private OptionUnit stringFormat;
        private List<IWithUpdatableOption<OutType>> withUpdateOption = new List<IWithUpdatableOption<OutType>>();

        private IOption parent;
        private IOption forceEnableCheckOption;
        private List<IOption> children = new List<IOption>();

        private bool enableInvert;
        private bool isHidden;
        private bool isHeader;
        private string name;
        private int curSelection;
        private int defaultSelection;
        private int id;
        private ConfigEntry<int> entry;

        private OptionBehaviour behaviour;

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
            IOption enableCheckOption)
        {
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

            this.children = new List<IOption>();
            this.forceEnableCheckOption = enableCheckOption;

            if (parent != null)
            {
                this.enableInvert = invert;
                parent.Children.Add(this);

            }

            this.curSelection = 0;
            if (id > 0)
            {
                this.entry = ExtremeRolesPlugin.Instance.Config.Bind(
                    OptionHolder.ConfigPreset, this.CleanedName, DefaultSelection);
                this.curSelection = Mathf.Clamp(this.entry.Value, 0, selections.Length - 1);
            }

            Logging.Debug($"OptinId:{this.Id}    Name:{this.Name}");

            OptionHolder.AllOption.Add(this.Id, this);
        }
        
        public virtual void Update(OutType newValue)
        {
            return;
        }

        public string GetName() => Translation.GetString(this.name);

        public string GetString()
        {
            string sel = Selections[CurSelection].ToString();
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
            if (this.IsHidden)
            {
                return false;
            }

            if (this.IsHeader || this.Parent == null)
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

            if (this.ForceEnableCheckOption != null)
            {
                active = active && this.ForceEnableCheckOption.Enabled;
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
            curSelection = Mathf.Clamp(
                (newSelection + Selections.Length) % Selections.Length,
                0, Selections.Length - 1);

            if (behaviour != null && behaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = CurSelection;
                stringOption.ValueText.text = this.GetString();
                if (this.withUpdateOption.Count != 0)
                {
                    foreach (IWithUpdatableOption<OutType> option in this.withUpdateOption)
                    {
                        option.Update(this.GetValue());
                    }
                }

                if (AmongUsClient.Instance?.AmHost == true && CachedPlayerControl.LocalPlayer)
                {
                    if (Id == 0)
                    {
                        OptionHolder.SwitchPreset(CurSelection); // Switch presets
                    }
                    else if (entry != null)
                    {
                        entry.Value = CurSelection; // Save selection to config
                    }

                    OptionHolder.ShareOptionSelections();// Share all selections
                }
            }
        }

        public void SaveConfigValue()
        {
            if (this.Entry != null)
            {
                this.Entry.Value = this.curSelection;
            }
        }

        public void SetOptionUnit(OptionUnit unit)
        {
            this.stringFormat = unit;
        }

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
            IOption enableCheckOption = null) : base(
                id, name,
                new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff",
                parent, isHeader, isHidden,
                format, invert,
                enableCheckOption)
        { }

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
            IOption enableCheckOption = null) : base(
                id, name,
                createSelection(min, max, step).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        { }

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
            IOption enableCheckOption = null) : base(
                id, name,
                createSelection(min, max, step).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        {
            this.minValue = Convert.ToInt32(this.Selections[0].ToString());
            this.maxValue = Convert.ToInt32(
                this.Selections[this.Selections.Length - 1].ToString());
        }

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
            IOption enableCheckOption = null) : base(
                id, name,
                createSelection(min, step, defaultValue).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        {
            this.step = step;
        }

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

        private static List<int> createSelection(int min, int step, int defaultValue)
        {
            List<int> selection = new List<int>();

            int tempMaxVale = (min + step) < defaultValue ? defaultValue : min + step;

            for (int s = min; s <= tempMaxVale; s += step)
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
            IOption enableCheckOption = null) : base(
                id, name,
                createSelection(min, step, defaultValue).ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        {
            this.step = step;
        }

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

        private static List<float> createSelection(float min, float step, float defaultValue)
        {

            List<float> selection = new List<float>();

            decimal dStep = new decimal(step);
            decimal dMin = new decimal(min);

            decimal tempMaxVale = (min + step) < defaultValue ? new decimal(defaultValue) : dMin + dStep;

            for (decimal s = dMin; s <= tempMaxVale; s += dStep)
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
            IOption enableCheckOption = null) : base(
                id, name, selections, "",
                parent, isHeader, isHidden,
                format, invert, enableCheckOption)
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


        public override dynamic GetValue() => CurSelection;
    }

    public static class CustomOption
    {
        public static string OptionToString(IOption option)
        {
            if (option == null) { return string.Empty; }
            if (!option.IsActive()) { return string.Empty; }
            return $"{option.GetName()}: {option.GetString()}";
        }

        public static string AllOptionToString(
            IOption option, bool skipFirst = false)
        {
            if (option == null) { return ""; }

            StringBuilder options = new StringBuilder();
            if (!option.IsHidden && !skipFirst)
            {
                options.AppendLine(OptionToString(option));
            }
            if (option.Enabled)
            {
                childrenOptionToString(option, ref options);
            }
            return options.ToString();
        }

        private static void childrenOptionToString(
            IOption option, ref StringBuilder options, int indentCount = 0)
        {
            foreach (IOption op in option.Children)
            {
                string str = OptionToString(op);

                if (str != string.Empty)
                {
                    if (indentCount != 0)
                    {
                        str = string.Concat(
                            string.Concat(
                                Enumerable.Repeat("    ", indentCount)),
                            str);
                    }

                    options.AppendLine(str);
                }
                childrenOptionToString(
                    op, ref options,
                    indentCount + 1);
            }
        }

    }

}

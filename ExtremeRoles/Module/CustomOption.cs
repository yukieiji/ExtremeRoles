using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

using BepInEx.Configuration;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module
{
    public enum OptionUnit
    {
        None = byte.MinValue,
        Second,
        Minute,
        Shot,
        Multiplier,
        Percentage,
    }

    public abstract class CustomOptionBase
    {
        public int Id;
        public int CurSelection;
        public int DefaultSelection;
        public string Name;

        public bool IsHeader;
        public bool IsHidden;
        public bool EnableInvert;

        public ConfigEntry<int> Entry;
        public CustomOptionBase Parent;
        public CustomOptionBase ForceEnableCheckOption = null;
        public OptionBehaviour Behaviour;
        public List<CustomOptionBase> Children;
        public object[] Selections;

        private OptionUnit stringFormat;
        private List<CustomOptionBase> withUpdateOption = new List<CustomOptionBase>();

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

        public CustomOptionBase(
            int id,
            string name,
            object[] selections,
            object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            OptionUnit format,
            bool invert,
            CustomOptionBase enableCheckOption)
        {
            int index = Array.IndexOf(selections, defaultValue);

            this.Id = id;
            this.Name = name;
            this.stringFormat = format;
            this.Selections = selections;
            this.DefaultSelection = index >= 0 ? index : 0;
            this.Parent = parent;
            this.IsHeader = isHeader;
            this.IsHidden = isHidden;
            this.EnableInvert = false;

            this.Children = new List<CustomOptionBase>();
            this.ForceEnableCheckOption = enableCheckOption;

            if (parent != null)
            {
                this.EnableInvert = invert;
                parent.Children.Add(this);

            }

            this.CurSelection = 0;
            if (id > 0)
            {
                this.Entry = ExtremeRolesPlugin.Instance.Config.Bind(
                    OptionHolder.ConfigPreset, this.CleanedName, DefaultSelection);
                this.CurSelection = Mathf.Clamp(this.Entry.Value, 0, selections.Length - 1);
            }

            Logging.Debug($"OptinId:{this.Id}    Name:{this.Name}");

            OptionHolder.AllOption.Add(this.Id, this);
        }
        
        protected virtual void OptionUpdate(object newValue)
        {
            return;
        }

        public string GetName() => Translation.GetString(Name);

        public object GetRawValue() => Selections[CurSelection];


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

            CustomOptionBase parent = this.Parent;
            bool active = true;

            while (parent != null && active)
            {
                active = parent.Enabled;
                parent = parent.Parent;
            }

            if (this.EnableInvert)
            {
                active = !active;
            }

            if (this.ForceEnableCheckOption != null)
            {
                active = active && this.ForceEnableCheckOption.Enabled;
            }

            return active;
        }

        public void SetUpdateOption(CustomOptionBase option)
        {
            this.withUpdateOption.Add(option);
        }

        public void UpdateSelection(int newSelection)
        {
            CurSelection = Mathf.Clamp(
                (newSelection + Selections.Length) % Selections.Length,
                0, Selections.Length - 1);

            if (Behaviour != null && Behaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = CurSelection;
                stringOption.ValueText.text = this.GetString();
                if (this.withUpdateOption.Count != 0)
                {
                    foreach (CustomOptionBase option in this.withUpdateOption)
                    {
                        option.OptionUpdate(this.GetValue());
                    }
                }

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (Id == 0)
                    {
                        OptionHolder.SwitchPreset(CurSelection); // Switch presets
                    }
                    else if (Entry != null)
                    {
                        Entry.Value = CurSelection; // Save selection to config
                    }

                    OptionHolder.ShareOptionSelections();// Share all selections
                }
            }
        }

        public void SaveConfigValue()
        {
            if (this.Entry != null)
            {
                this.Entry.Value = this.CurSelection;
            }
        }

        public abstract dynamic GetValue();
    }

    public class BoolCustomOption : CustomOptionBase
    {
        public BoolCustomOption(
            int id, string name,
            bool defaultValue,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name,
                new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff",
                parent, isHeader, isHidden,
                format, invert,
                enableCheckOption)
        { }

        public override dynamic GetValue() => CurSelection > 0;
    }

    public class FloatCustomOption : CustomOptionBase
    {
        public FloatCustomOption(
            int id, string name,
            float defaultValue,
            float min, float max, float step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name,
                createSelection(min, max, step).Cast<object>().ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        { }

        public override dynamic GetValue() => (float)GetRawValue();

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

    public class IntCustomOption : CustomOptionBase
    {
        private int maxValue;
        private int minValue;

        public IntCustomOption(
            int id,
            string name,
            int defaultValue,
            int min, int max, int step,
            CustomOptionBase parent = null,
            bool isHeader = false, 
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name,
                createSelection(min, max, step).Cast<object>().ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        {
            this.minValue = Convert.ToInt32(this.Selections[0].ToString());
            this.maxValue = Convert.ToInt32(
                this.Selections[this.Selections.Length - 1].ToString());
        }

        public override dynamic GetValue() => Convert.ToInt32(GetRawValue().ToString());

        protected override void OptionUpdate(object newValue)
        {
            int newIntedValue = Convert.ToInt32(newValue.ToString());
            int newMaxValue = this.maxValue / newIntedValue;

            List<int> newSelections = new List<int>();
            for (int s = minValue; s <= newMaxValue; ++s)
            {
                newSelections.Add(s);
            }

            this.Selections = newSelections.Cast<object>().ToArray();
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

    public class IntDynamicCustomOption : CustomOptionBase
    {
        public IntDynamicCustomOption(
            int id, string name,
            int defaultValue,
            int min, int step,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name,
                createSelection(min, step).Cast<object>().ToArray(),
                defaultValue, parent,
                isHeader, isHidden,
                format, invert,
                enableCheckOption)
        { }

        public override dynamic GetValue() => Convert.ToInt32(GetRawValue().ToString());

        protected override void OptionUpdate(object newValue)
        {
            int maxValue = Convert.ToInt32(newValue.ToString());
            int minValue = Convert.ToInt32(this.Selections[0].ToString());

            List<int> newSelections = new List<int>();
            for (int s = minValue; s < maxValue; s += 1)
            {
                newSelections.Add(s);
            }
            this.Selections = newSelections.Cast<object>().ToArray();
            this.UpdateSelection(this.CurSelection);
        }

        private static List<int> createSelection(int min, int step)
        {
            List<int> selection = new List<int>();
            for (int s = min; s <= min + 1; s += step)
            {
                selection.Add(s);
            }

            return selection;
        }

    }


    public class SelectionCustomOption : CustomOptionBase
    {
        public SelectionCustomOption(
            int id, string name,
            string[] selections,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name, selections, "",
                parent, isHeader, isHidden,
                format, invert, enableCheckOption)
        { }

        public SelectionCustomOption(
            int id, string name,
            string[] selections,
            int defaultIndex,
            CustomOptionBase parent = null,
            bool isHeader = false,
            bool isHidden = false,
            OptionUnit format = OptionUnit.None,
            bool invert = false,
            CustomOptionBase enableCheckOption = null) : base(
                id, name, selections, defaultIndex,
                parent, isHeader, isHidden,
                format, invert, enableCheckOption)
        { }


        public override dynamic GetValue() => CurSelection;
    }

    public static class CustomOption
    {
        public static string OptionToString(CustomOptionBase option)
        {
            if (option == null) { return string.Empty; }
            if (!option.IsActive()) { return string.Empty; }
            return $"{option.GetName()}: {option.GetString()}";
        }

        public static string AllOptionToString(
            CustomOptionBase option, bool skipFirst = false)
        {
            if (option == null) { return ""; }

            List<string> options = new List<string>();
            if (!option.IsHidden && !skipFirst)
            {
                options.Add(OptionToString(option));
            }
            if (option.Enabled)
            {
                childrenOptionToString(option, ref options);
            }
            return string.Join("\n", options);
        }

        public static void childrenOptionToString(
            CustomOptionBase option, ref List<string> options, int indentCount = 0)
        {
            foreach (CustomOptionBase op in option.Children)
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

                    options.Add(str);
                }
                childrenOptionToString(
                    op, ref options,
                    indentCount + 1);
            }
        }

    }

}

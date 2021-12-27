using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx.Configuration;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Module
{
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
        public OptionBehaviour Behaviour;
        public List<CustomOptionBase> Children;
        public System.Object[] Selections;

        private string stringFormat;
        private List<CustomOptionBase> withUpdateOption = new List<CustomOptionBase>();

        public virtual bool Enabled
        {
            get
            {
                return this.GetBool();
            }
        }

        public CustomOptionBase(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert)
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

            if (parent != null)
            {
                this.EnableInvert = invert;
                parent.Children.Add(this);
            }

            CurSelection = 0;
            if (id > 0)
            {
                Entry = ExtremeRolesPlugin.Instance.Config.Bind(
                    $"Preset{OptionsHolder.SelectedPreset}", id.ToString(), DefaultSelection);
                CurSelection = Mathf.Clamp(Entry.Value, 0, selections.Length - 1);
            }

            Logging.Debug($"OptinId:{this.Id}    Name:{this.Name}");

            OptionsHolder.AllOptions.Add(this.Id, this);
        }

        protected bool GetBool() => CurSelection > 0;
        
        protected virtual void OptionUpdate(object newValue)
        {
            return;
        }

        public string GetName() => Translation.GetString(Name);

        public object GetRawValue() => Selections[CurSelection];


        public string GetString()
        {
            string sel = Selections[CurSelection].ToString();
            if (this.stringFormat != "")
            {
                return string.Format(
                    Translation.GetString(this.stringFormat), sel);
            }
            return Translation.GetString(sel);
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
                stringOption.ValueText.text = Selections[CurSelection].ToString();
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
                        OptionsHolder.SwitchPreset(CurSelection); // Switch presets
                    }
                    else if (Entry != null)
                    {
                        Entry.Value = CurSelection; // Save selection to config
                    }

                    OptionsHolder.ShareOptionSelections();// Share all selections
                }
            }
        }

        public abstract dynamic GetValue();
    }

    public class BoolCustomOption : CustomOptionBase
    {
        public BoolCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format, invert)
        {}
        public override dynamic GetValue() => GetBool();
    }

    public class FloatCustomOption : CustomOptionBase
    {
        public FloatCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format, invert)
        { }
        public override dynamic GetValue() => (float)GetRawValue();
    }

    public class IntCustomOption : CustomOptionBase
    {
        private int maxValue;
        private int minValue;

        public IntCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format, invert)
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

            Logging.Debug($"newValue:{newMaxValue}");

            List<int> newSelections = new List<int>();
            for (int s = minValue; s <= newMaxValue; ++s)
            {
                newSelections.Add(s);
            }

            Logging.Debug($"newSelectionLength:{newSelections.Count}");

            this.Selections = newSelections.Cast<object>().ToArray();
            this.UpdateSelection(this.CurSelection);
        }
    }

    public class IntDynamicCustomOption : CustomOptionBase
    {
        public IntDynamicCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format, invert)
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
    }


    public class SelectionCustomOption : CustomOptionBase
    {
        public SelectionCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format,
            bool invert) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format, invert)
        { }
        public override dynamic GetValue() => CurSelection;
    }


    public static class CustomOption
    {
        public static CustomOptionBase Create(
            int id, string name, string[] selections,
            CustomOptionBase parent = null, bool isHeader = false,
            bool isHidden = false, string format = "", bool invert=false)
        {
            return new SelectionCustomOption(
                id, name, selections, "",
                parent, isHeader, isHidden,
                format, invert);
        }
        public static CustomOptionBase Create(
            int id, string name, string[] selections,
            int defaultIndex, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false,
            string format = "", bool invert = false)
        {
            return new SelectionCustomOption(
                id, name, selections, defaultIndex,
                parent, isHeader, isHidden, format, invert);
        }

        public static CustomOptionBase Create
            (int id, string name, int defaultValue,
            int min, int max, int step, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false,
            string format = "", bool invert = false)
        {
            List<int> selections = new List<int>();
            for (int s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new IntCustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden,
                format, invert);
        }

        public static CustomOptionBase Create
            (int id, string name, int defaultValue,
            int min, int step, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false,
            string format = "", bool invert = false)
        {
            List<int> selections = new List<int>();
            for (int s = min; s <= min + 1; s += step)
            {
                selections.Add(s);
            }
            return new IntDynamicCustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format, invert);
        }

        public static CustomOptionBase Create
            (int id, string name, float defaultValue,
            float min, float max, float step, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false,
            string format = "", bool invert = false)
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new FloatCustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format, invert);
        }

        public static CustomOptionBase Create(
            int id, string name, bool defaultValue,
            CustomOptionBase parent = null, bool isHeader = false,
            bool isHidden = false, string format = "", bool invert = false)
        {
            return new BoolCustomOption(
                id, name, new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff", parent,
                isHeader, isHidden, format, invert);
        }
    }

}

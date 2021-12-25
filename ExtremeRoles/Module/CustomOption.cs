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
        public string Name;
        public string Format;
        public System.Object[] Selections;

        public int DefaultSelection;
        public ConfigEntry<int> Entry;
        public int CurSelection;
        public OptionBehaviour Behaviour;
        public CustomOptionBase Parent;
        public List<CustomOptionBase> Children;
        public bool IsHeader;
        public bool IsHidden;

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
            string format)
        {
            int index = Array.IndexOf(selections, defaultValue);

            this.Id = id;
            this.Name = name;
            this.Format = format;
            this.Selections = selections;
            this.DefaultSelection = index >= 0 ? index : 0;
            this.Parent = parent;
            this.IsHeader = isHeader;
            this.IsHidden = isHidden;

            this.Children = new List<CustomOptionBase>();

            if (parent != null)
            {
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

        public string GetName() => Translation.GetString(Name);

        public object GetRawValue() => Selections[CurSelection];

        public string GetString()
        {
            string sel = Selections[CurSelection].ToString();
            if (Format != "")
            {
                return string.Format(Translation.GetString(Format), sel);
            }
            return Translation.GetString(sel);
        }

        public void UpdateSelection(int newSelection)
        {
            CurSelection = Mathf.Clamp((newSelection + Selections.Length) % Selections.Length, 0, Selections.Length - 1);
            if (Behaviour != null && Behaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = CurSelection;
                stringOption.ValueText.text = Selections[CurSelection].ToString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (Id == 0) OptionsHolder.SwitchPreset(CurSelection); // Switch presets
                    else if (Entry != null) Entry.Value = CurSelection; // Save selection to config

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
            string format) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format)
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
            string format) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format)
        { }
        public override dynamic GetValue() => (float)GetRawValue();
    }

    public class IntCustomOption : CustomOptionBase
    {
        public IntCustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOptionBase parent,
            bool isHeader,
            bool isHidden,
            string format) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format)
        { }
        public override dynamic GetValue() => Convert.ToInt32(GetRawValue().ToString());
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
            string format) : base(
                id, name, selections,
                defaultValue, parent,
                isHeader, isHidden,
                format)
        { }
        public override dynamic GetValue() => CurSelection;
    }


    public static class CustomOption
    {
        public static CustomOptionBase Create(
            int id, string name, string[] selections,
            CustomOptionBase parent = null, bool isHeader = false,
            bool isHidden = false, string format = "")
        {
            return new SelectionCustomOption(
                id, name, selections, "",
                parent, isHeader, isHidden, format);
        }
        public static CustomOptionBase Create(
            int id, string name, string[] selections,
            int defaultIndex, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new SelectionCustomOption(
                id, name, selections, defaultIndex,
                parent, isHeader, isHidden, format);
        }

        public static CustomOptionBase Create
            (int id, string name, int defaultValue,
            int min, int max, int step, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<int> selections = new List<int>();
            for (int s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new IntCustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format);
        }
        public static CustomOptionBase Create
            (int id, string name, float defaultValue,
            float min, float max, float step, CustomOptionBase parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new FloatCustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format);
        }

        public static CustomOptionBase Create(
            int id, string name, bool defaultValue,
            CustomOptionBase parent = null, bool isHeader = false,
            bool isHidden = false, string format = "")
        {
            return new BoolCustomOption(
                id, name, new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff", parent,
                isHeader, isHidden, format);
        }
    }

}

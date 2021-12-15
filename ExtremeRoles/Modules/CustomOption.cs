using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using BepInEx.Configuration;

namespace ExtremeRoles.Modules
{

    public class CustomOption
    {

        public int Id;
        public string Name;
        public string Format;
        public System.Object[] Selections;

        public int DefaultSelection;
        public ConfigEntry<int> Entry;
        public int CurSelection;
        public OptionBehaviour Behaviour;
        public CustomOption Parent;
        public List<CustomOption> Children;
        public bool IsHeader;
        public bool IsHidden;

        public virtual bool Enabled
        {
            get
            {
                return this.GetBool();
            }
        }

        public CustomOption()
        {}

        public CustomOption(
            int id,
            string name,
            System.Object[] selections,
            System.Object defaultValue,
            CustomOption parent,
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

            this.Children = new List<CustomOption>();
            
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

            Helpers.DebugLog($"OptinId:{this.Id}    Name:{this.Name}");

            OptionsHolder.AllOptions.Add(this.Id, this);
        }

        public static CustomOption Create(
            int id, string name, string[] selections,
            CustomOption parent = null, bool isHeader = false,
            bool isHidden = false, string format = "")
        {
            return new CustomOption(
                id, name, selections, "",
                parent, isHeader, isHidden, format);
        }
        public static CustomOption Create(
            int id, string name, string[] selections,
            int defaultIndex, CustomOption parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(
                id, name, selections, defaultIndex,
                parent, isHeader, isHidden, format);
        }

        public static CustomOption Create
            (int id, string name, int defaultValue,
            int min, int max, int step, CustomOption parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<int> selections = new List<int>();
            for (int s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new CustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format);
        }
        public static CustomOption Create
            (int id, string name, float defaultValue,
            float min, float max, float step, CustomOption parent = null,
            bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
            {
                selections.Add(s);
            }
            return new CustomOption(
                id, name, selections.Cast<object>().ToArray(),
                defaultValue, parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(
            int id, string name, bool defaultValue,
            CustomOption parent = null, bool isHeader = false,
            bool isHidden = false, string format = "")
        {
            return new CustomOption(
                id, name, new string[] { "optionOff", "optionOn" },
                defaultValue ? "optionOn" : "optionOff", parent,
                isHeader, isHidden, format);
        }

        public virtual bool GetBool() => CurSelection > 0;

        public virtual float GetFloat() => (float)GetRawValue();

        public virtual int GetInt() => Convert.ToInt32(GetRawValue().ToString());

        public virtual string GetName() => Translation.GetString(Name);

        public virtual int GetPercentage() => (int)Decimal.Multiply(CurSelection, Selections.ToList().Count);

        public virtual object GetRawValue() => Selections[CurSelection];

        public virtual int GetSelection() => CurSelection;

        public virtual string GetString()
        {
            string sel = Selections[CurSelection].ToString();
            if (Format != "")
            {
                return string.Format(Translation.GetString(Format), sel);
            }
            return Translation.GetString(sel);
        }

        public virtual void UpdateSelection(int newSelection)
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
    }

    public class BlankOption : CustomOption
    {
        public BlankOption(CustomOption parent)
        {
            this.Parent = parent;
            this.Id = -1;
            this.Name = "";
            this.IsHeader = false;
            this.IsHidden = true;
            this.Children = new List<CustomOption>();
            this.Selections = new string[] { "" };
            OptionsHolder.AllOptions.Add(this.Id, this);
        }

        public override bool GetBool() => true;

        public override float GetFloat() => 0f;

        public override int GetPercentage() => 0;

        public override int GetSelection() => 0;

        public override string GetString() => "";

        public override void UpdateSelection(int newSelection)
        {
            return;
        }

    }

}

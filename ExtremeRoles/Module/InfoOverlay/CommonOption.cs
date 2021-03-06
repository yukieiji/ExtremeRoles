using System;
using System.Collections.Generic;

namespace ExtremeRoles.Module.InfoOverlay
{
    public static class CommonOption
    {
        public static string GetGameOptionString()
        {

            var allOption = OptionHolder.AllOption;

            List<string> printOption = new List<string>();

            foreach (OptionHolder.CommonOptionKey key in Enum.GetValues(
                typeof(OptionHolder.CommonOptionKey)))
            {
                if (key == OptionHolder.CommonOptionKey.PresetSelection) { continue; }

                if (key == OptionHolder.CommonOptionKey.NumMeating)
                {
                    printOption.Add("");
                }

                var option = allOption[(int)key];

                if (option == null) { continue; }

                if (!option.IsHidden)
                {
                    printOption.Add(
                        CustomOption.OptionToString(option));
                }
                if (option.Enabled)
                {
                    foreach (CustomOptionBase op in option.Children)
                    {
                        string str = CustomOption.OptionToString(op);
                        if (str != "")
                        {
                            printOption.Add(str);
                        }
                    }
                }

            }

            return string.Join("\n", printOption);

        }
    }
}

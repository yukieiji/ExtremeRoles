using System;
using System.Text;

namespace ExtremeRoles.Module.InfoOverlay
{
    public static class CommonOption
    {
        public static string GetGameOptionString()
        {

            var allOption = OptionHolder.AllOption;

            StringBuilder printOption = new StringBuilder();

            foreach (OptionHolder.CommonOptionKey key in Enum.GetValues(
                typeof(OptionHolder.CommonOptionKey)))
            {
                if (key == OptionHolder.CommonOptionKey.PresetSelection) { continue; }

                if (key == OptionHolder.CommonOptionKey.NumMeating)
                {
                    printOption.AppendLine("");
                }

                var option = allOption[(int)key];

                if (option == null) { continue; }

                if (!option.IsHidden)
                {
                    string optStr = option.ToHudString();
                    if (optStr != string.Empty)
                    {
                        printOption.AppendLine(optStr);
                    }
                }
            }

            return printOption.ToString();

        }
    }
}

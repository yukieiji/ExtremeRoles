using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module
{
    public static class CustomOptionCsvProcessor
    {
        private const string csvName = "option.csv";

        private const string modName = "Extreme Roles";
        private const string versionStr = "Version";

        private const string comma = ",";

        public static bool Export()
        {
            Helper.Logging.Debug("Export Start!!!!!!");
            try
            {
                using (var csv = new StreamWriter(csvName, false, new UTF8Encoding(true)))
                {
                    csv.WriteLine(
                        string.Format("{1}{0}{2}{0}{3}{0}{4}",
                            comma, "Name", "OptionValue", "CustomOptionName", "SelectedIndex")); //ヘッダー


                    foreach (IOption option in OptionHolder.AllOption.Values)
                    {

                        if (option.Id == 0) { continue; }

                        csv.WriteLine(
                            string.Format("{1}{0}{2}{0}{3}{0}{4}",
                                comma,
                                clean(option.GetTranedName()),
                                clean(option.GetString()),
                                clean(option.Name),
                                option.CurSelection));
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Helper.Logging.Error(e.ToString());
            }
            return false;
        }

        public static bool Import()
        {
            try
            {

                Helper.Logging.Debug("Import Start!!!!!!");

                Dictionary<string, int> importedOption = new Dictionary<string, int>();

                using (var csv = new StreamReader(csvName, new UTF8Encoding(true)))
                {

                    string line = csv.ReadLine(); // ヘッダー
                    while ((line = csv.ReadLine()) != null)
                    {
                        string[] option = line.Split(',');

                        importedOption.Add(
                            option[2], // cleanedName
                            int.Parse(option[3])); // selection
                    }

                }

                foreach (IOption option in OptionHolder.AllOption.Values)
                {

                    if (option.Id == 0) { continue; }

                    if (importedOption.TryGetValue(
                        clean(option.Name),
                        out int selection))
                    {
                        option.UpdateSelection(selection);
                        option.SaveConfigValue();
                    }
                }

                Helper.Logging.Debug("Import Comp!!!!!!");

                if (AmongUsClient.Instance?.AmHost == true && CachedPlayerControl.LocalPlayer)
                {
                    OptionHolder.ShareOptionSelections();// Share all selections
                }

                return true;

            }
            catch (Exception newE)
            {
                try
                {
                    return prevOptionCsvLoad();
                }
                catch (Exception prevE)
                {
                    Helper.Logging.Error($"prev csv load error:{prevE}");
                }

                Helper.Logging.Error($"Newed csv load error:{newE}");
            }
            return false;
        }

        private static bool prevOptionCsvLoad()
        {
            using (var csv = new StreamReader(csvName, new UTF8Encoding(true)))
            {
                string line = csv.ReadLine(); // バージョン情報
                string[] varsionHeader = line.Split(comma);

                if (varsionHeader[0].Equals(modName) &&
                    varsionHeader[1].Equals(versionStr) &&
                    varsionHeader[2].Equals(
                        Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                {

                    csv.ReadLine(); // ヘッダー


                    while ((line = csv.ReadLine()) != null)
                    {
                        string[] option = line.Split(',');

                        int id = int.Parse(option[0]);
                        int selection = int.Parse(option[3]);

                        if (id == 0) { continue; }

                        if (OptionHolder.AllOption.ContainsKey(id))
                        {
                            OptionHolder.AllOption[id].UpdateSelection(selection);
                            OptionHolder.AllOption[id].SaveConfigValue();
                        }
                    }
                }
                else
                {
                    return false;
                }

            }

            Helper.Logging.Debug("Import Comp!!!!!!");

            if (AmongUsClient.Instance?.AmHost == true && CachedPlayerControl.LocalPlayer)
            {
                OptionHolder.ShareOptionSelections();// Share all selections
            }
            return true;
        }

        private static string clean(string value)
        {
            value = Regex.Replace(value, "<.*?>", string.Empty);
            value = Regex.Replace(value, "^-\\s*", string.Empty);
            value = Regex.Replace(value, "\\\n", string.Empty);

            return value.Trim();
        }
    }
}

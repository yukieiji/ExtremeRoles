using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtremeRoles.Module
{
    public class CustomOptionCsvProcessor
    {
        private const string modName = "Extreme Roles";
        private const string versionStr = "Version";

        private const string comma = ",";

        public static bool Export()
        {
            Helper.Logging.Debug("Export Start!!!!!!");
            try
            {
                using (var csv = new StreamWriter("option.csv", false, new UTF8Encoding(true)))
                {
                    csv.WriteLine(
                        string.Format("{1}{0}{2}{0}{3}",
                        comma,
                        modName,
                        versionStr,
                        Assembly.GetExecutingAssembly().GetName().Version));
                    csv.WriteLine(
                        string.Format("{1}{0}{2}{0}{3}{0}{4}",
                            comma, "Id", "Name", "Option Value", "SelectedIndex")); //ヘッダー


                    foreach (var (_, option) in OptionHolder.AllOption)
                    {
                        csv.WriteLine(
                            string.Format("{1}{0}{2}{0}{3}{0}{4}",
                                comma,
                                option.Id,
                                clean(option.GetName()),
                                clean(option.GetString()),
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

                using (var csv = new StreamReader("option.csv", new UTF8Encoding(true)))
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
                            if (OptionHolder.AllOption.ContainsKey(id))
                            {
                                OptionHolder.AllOption[id].UpdateSelection(selection);
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                }

                Helper.Logging.Debug("Import Comp!!!!!!");

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    OptionHolder.ShareOptionSelections();// Share all selections
                }

                return true;

            }
            catch (Exception e)
            {
                Helper.Logging.Error(e.ToString());
            }
            return false;
        }

        private static string clean(string value)
        {
            value = Regex.Replace(value, "<.*?>", "");
            return value.Trim();
        }
    }
}

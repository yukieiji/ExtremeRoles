
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtremeRoles.Module
{
    internal class ExportableCustomOption
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public int SelectedIndex { get; set; }
    }

    public class CustomOptionCsvProcessor
    {

        private const string comma = ",";

        public static void Export()
        {

            Helper.Logging.Debug("Export Start!!!!!!");

            using (var csv = new StreamWriter("option.csv", false, new UTF8Encoding(true)))
            {
                csv.WriteLine(
                    string.Format("{1}{0}{2}{0}{3}",
                    comma,
                    "Extreme Roles",
                    "Version",
                    Assembly.GetExecutingAssembly().GetName().Version));
                csv.WriteLine(
                    string.Format("{1}{0}{2}{0}{3}{0}{4}",
                        comma, "Id", "Name", "Option Value", "SelectedIndex")); //ヘッダー
               

                foreach (var (_, option) in OptionsHolder.AllOption)
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
        }

        public static void Import()
        {
            try
            {

                Helper.Logging.Debug("Import Start!!!!!!");

                using (var csv = new StreamReader("option.csv", new UTF8Encoding(true)))
                {
                    string line = csv.ReadLine();
                    string[] varsionHeader = line.Split(comma);

                    if (varsionHeader[0].Equals("Extreme Roles") &&
                        varsionHeader[1].Equals("Version") &&
                        varsionHeader[2].Equals(
                            Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                    {

                        csv.ReadLine(); // ヘッダー

                        while ((line = csv.ReadLine()) != null)
                        {
                            string[] option = line.Split(',');

                            int id = int.Parse(option[0]);
                            int selection = int.Parse(option[3]);
                            if (OptionsHolder.AllOption.ContainsKey(id))
                            {
                                OptionsHolder.AllOption[id].UpdateSelection(selection);
                            }

                        }
                    }

                }

                Helper.Logging.Debug("Import Comp!!!!!!");

            }
            catch (Exception e)
            {
                Helper.Logging.Debug(e.ToString());
            }
        }

        private static string clean(string value)
        {
            value = Regex.Replace(value, "<.*?>", "");
            return value.Trim();
        }
    }
}

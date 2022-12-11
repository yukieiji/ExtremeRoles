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

        private const string vanilaOptionKey = "BytedVanillaOptions";

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
                           comma,
                           "Game Infos",
                           $"AmongUs ver.{UnityEngine.Application.version}",
                           $"ExtremeRoles ver.{Assembly.GetExecutingAssembly().GetName().Version}",
                           $"Exported on:{DateTime.UtcNow}"));

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

                    csv.WriteLine(
                        string.Format(
                            "{1}{0}{1}",comma, string.Empty));
                    /*
                    foreach (byte bytedOption in PlayerControl.GameOptions.ToBytes(6))
                    {
                        csv.WriteLine(
                            string.Format("{1}{0}{2}",
                                comma,
                                vanilaOptionKey,
                                bytedOption));
                    }
                    */
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

                ExtremeRolesPlugin.Logger.LogInfo("---------- Option Import Start ----------");

                Dictionary<string, int> importedOption = new Dictionary<string, int>();
                List<byte> importedVanillaOptions = new List<byte>();

                using (var csv = new StreamReader(csvName, new UTF8Encoding(true)))
                {
                    string infoData = csv.ReadLine(); // verHeader
                    string[] info = infoData.Split(',');

                    ExtremeRolesPlugin.Logger.LogInfo(
                        $"Loading from {info[1]} with {info[2]} {info[3]} Data");

                    string line = csv.ReadLine(); // ヘッダー
                    while ((line = csv.ReadLine()) != null)
                    {
                        string[] option = line.Split(',');

                        if (option[0] == string.Empty)
                        { 
                            continue; 
                        }
                        else if (option[0] == vanilaOptionKey)
                        {
                            importedVanillaOptions.Add(
                                byte.Parse(option[1]));
                        }
                        else
                        {
                            importedOption.Add(
                                option[2], // cleanedName
                                int.Parse(option[3])); // selection
                        }
                    }

                }
                /*
                if (importedVanillaOptions.Count > 0)
                {
                    PlayerControl.GameOptions = GameOptionsData.FromBytes(
                        importedVanillaOptions.ToArray());
                }
                */

                // オプションのインポートデモでネットワーク帯域とサーバーに負荷をかけて人が落ちたりするので共有を一時的に無効化して実行
                OptionHolder.ExecuteWithBlockOptionShare(
                    () =>
                    {
                        foreach (IOption option in OptionHolder.AllOption.Values)
                        {

                            if (option.Id == 0) { continue; }

                            if (importedOption.TryGetValue(
                                clean(option.Name),
                                out int selection))
                            {
                                ExtremeRolesPlugin.Logger.LogInfo(
                                    $"Update Option : {option.Name} to Selection:{selection}");
                                option.UpdateSelection(selection);
                                option.SaveConfigValue();
                            }
                        }
                    });

                if (AmongUsClient.Instance?.AmHost == true && CachedPlayerControl.LocalPlayer)
                {
                    OptionHolder.ShareOptionSelections();// Share all selections
                }

                ExtremeRolesPlugin.Logger.LogInfo("---------- Option Import Complete ----------");

                return true;

            }
            catch (Exception newE)
            {

                ExtremeRolesPlugin.Logger.LogInfo($"Newed csv load error:{newE}");
            }
            return false;
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

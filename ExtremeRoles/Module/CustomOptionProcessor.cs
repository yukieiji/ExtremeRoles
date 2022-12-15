using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using AmongUs.GameOptions;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module
{
    public static class CustomOptionCsvProcessor
    {
        private const string csvName = "option.csv";

        private const string vanilaOptionKey = "BytedVanillaOptions";

        private const string comma = ",";

        private const int curVersion = 7;

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

                    var gameOptionManager = GameOptionsManager.Instance;

                    foreach (GameModes gameMode in Enum.GetValues(typeof(GameModes)))
                    {
                        IGameOptions option = gameMode switch
                        {
                            GameModes.Normal => 
                                gameOptionManager.normalGameHostOptions.Cast<IGameOptions>(),
                            GameModes.HideNSeek =>
                                gameOptionManager.hideNSeekGameHostOptions.Cast<IGameOptions>(),
                            _ => null,
                        };
                        if (option == null) { continue; }
                        exportIGameOptions(csv, gameOptionManager.gameOptionsFactory, option, gameMode);
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

                ExtremeRolesPlugin.Logger.LogInfo("---------- Option Import Start ----------");

                Dictionary<string, int> importedOption = new Dictionary<string, int>();
                Dictionary<GameModes, List<byte>> importedVanillaOptions = 
                    new Dictionary<GameModes, List<byte>>();

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
                            GameModes mode = (GameModes)Enum.Parse(typeof(GameModes), option[1]);

                            if (!importedVanillaOptions.TryGetValue(
                                    mode, out List<byte> modeOption))
                            {
                                modeOption = new List<byte>();
                                importedVanillaOptions.Add(mode, modeOption);
                            }

                            modeOption.Add(byte.Parse(option[2]));
                        }
                        else
                        {
                            importedOption.Add(
                                option[2], // cleanedName
                                int.Parse(option[3])); // selection
                        }
                    }

                }

                var gameOptionManager = GameOptionsManager.Instance;

                foreach (var (mode, bytedOptions) in importedVanillaOptions)
                {
                    IGameOptions option = gameOptionManager.gameOptionsFactory.FromBytes(
                        bytedOptions.ToArray());

                    if (option == null) { continue; }
                    
                    switch (mode)
                    {
                        case GameModes.Normal:

                            NormalGameOptionsV07 normalOption = option.Cast<NormalGameOptionsV07>();

                            if (option.Version < curVersion)
                            {
                                normalOption = gameOptionManager.MigrateNormalGameOptions(option);
                            }
                            
                            if (normalOption == null) { continue; }

                            gameOptionManager.normalGameHostOptions = normalOption;
                            gameOptionManager.SaveNormalHostOptions();
                            break;
                        case GameModes.HideNSeek:
                            HideNSeekGameOptionsV07 hideNSeekOption = option.Cast<HideNSeekGameOptionsV07>();

                            if (option.Version < curVersion)
                            {
                                hideNSeekOption = gameOptionManager.MigrateHideNSeekGameOptions(option);
                            }

                            if (hideNSeekOption == null) { continue; }

                            gameOptionManager.hideNSeekGameHostOptions = hideNSeekOption;
                            gameOptionManager.SaveHideNSeekHostOptions();
                            break;
                        default:
                            break;
                    }
                }

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

        private static void exportIGameOptions(
            StreamWriter writer,
            GameOptionsFactory factory,
            IGameOptions option, GameModes mode)
        {
            foreach (byte bytedOption in factory.ToBytes(option))
            {
                writer.WriteLine(
                    string.Format("{1}{0}{2}{0}{3}",
                        comma,
                        vanilaOptionKey,
                        mode.ToString(),
                        bytedOption));
            }
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

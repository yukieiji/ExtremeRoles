using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using BepInEx.Configuration;

using UnityEngine;

using ExtremeSkins.Module;

namespace ExtremeSkins
{
    public class CreatorModeManager
    {
        public enum Mode
        {
            Enable,
            Disable,
            EnableReady,
            DisableReady
        }

        public static CreatorModeManager Instance;

        public bool IsEnable => this.creatorModeConfig.Value;
        public string StatusString => this.statusString;

        private ConfigEntry<bool> creatorModeConfig;
        private Mode mode;
        private string statusString = string.Empty;

        private const string additionalTransCsv = "translation.csv";
        private const string additionalColorCsv = "color.csv";

        private const string folder = "CreatorMode";
        private const string slash = @"\";

        private const string comma = ",";

        public CreatorModeManager()
        {
            this.creatorModeConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "CreatorMode", false);

            this.mode = this.IsEnable ? Mode.Enable : Mode.Disable;
            updateStatusString();
        }

        public void SwitchMode()
        {
            this.mode = this.IsEnable ? Mode.DisableReady : Mode.EnableReady;
            this.creatorModeConfig.Value = !this.IsEnable;
            ExtremeSkinsPlugin.Logger.LogInfo($"Switch Mode: {this.mode}");
            updateStatusString();
        }

        private void updateStatusString()
        {
            this.statusString = this.mode switch
            {
                Mode.Enable       => Helper.Translation.GetString("enableCreatorMode"),
                Mode.DisableReady => Helper.Translation.GetString("disableReadyCreatorMode"),
                Mode.EnableReady  => Helper.Translation.GetString("enableReadyCreatorMode"),
                _ => string.Empty
            };
        }

        public static void Initialize()
        {
            Instance = new CreatorModeManager();

            if (Instance.IsEnable)
            {
                string creatorModePath = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    slash, folder);

                if (!Directory.Exists(creatorModePath))
                {
                    Directory.CreateDirectory(creatorModePath);
                }

                tryImportTestTransData(creatorModePath);
                tryImportTestColor(creatorModePath);
            }
        }

        private static void tryImportTestTransData(string workDir)
        {
            string transCsvPath = string.Concat(
                workDir, slash, additionalTransCsv);
            if (File.Exists(transCsvPath))
            {
                using StreamReader transCsv = new StreamReader(
                    transCsvPath, new UTF8Encoding(true));

                transCsv.ReadLine(); // verHeader

                string transInfoLine;
                while ((transInfoLine = transCsv.ReadLine()) != null)
                {
                    string[] transInfo = transInfoLine.Split(',');

                    Dictionary<SupportedLangs, string> transData = 
                        new Dictionary<SupportedLangs, string>();

                    foreach (var (str, index) in transInfo.Select(
                        (str, index) => (str, index)))
                    {
                        if (index == 0 || str == string.Empty) { continue; }
                        transData.Add((SupportedLangs)(index - 1), str);
                    }

                    Helper.Translation.AddKeyTransdata(
                        transInfo[0], transData);
                }
            }
            else
            {
                List<string> langList = new List<string>();

                foreach (SupportedLangs enumValue in Enum.GetValues(typeof(SupportedLangs)))
                {
                    langList.Add(enumValue.ToString());
                }

                using StreamWriter transCsv = new StreamWriter(
                    transCsvPath, false, new UTF8Encoding(true));
                transCsv.WriteLine(
                   string.Format(
                       "{1}{0}{2}",
                       comma,
                       "TransKey",
                       string.Join(comma, langList)));
            }
        }

        private static void tryImportTestColor(string workDir)
        {
            string colorCsvPath = string.Concat(
                workDir, slash, additionalColorCsv);

            if (File.Exists(colorCsvPath))
            {
                using StreamReader colorCsv = new StreamReader(
                    colorCsvPath, new UTF8Encoding(true));
                colorCsv.ReadLine(); // verHeader

                string colorInfoLine;
                while ((colorInfoLine = colorCsv.ReadLine()) != null)
                {
                    string[] colorInfo = colorInfoLine.Split(',');

                    CustomColorPalette.ColorData color = new CustomColorPalette.ColorData()
                    {
                        Name = colorInfo[0],
                        MainColor = new Color32(
                            byte.Parse(colorInfo[1]),
                            byte.Parse(colorInfo[2]),
                            byte.Parse(colorInfo[3]),
                            byte.Parse(colorInfo[4])),
                        ShadowColor = new Color32(
                            byte.Parse(colorInfo[6]),
                            byte.Parse(colorInfo[7]),
                            byte.Parse(colorInfo[8]),
                            byte.Parse(colorInfo[9])),
                    };

                    CustomColorPalette.AddCustomColor(color);
                }
            }
            else
            {
                using StreamWriter colorCsv = new StreamWriter(
                    colorCsvPath, false, new UTF8Encoding(true));
                colorCsv.WriteLine(
                   string.Format(
                       "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{0}{6}{0}{7}{0}{8}{0}{9}",
                       comma,
                       "ColorName",
                       "MainColor R(0-255)",
                       "MainColor G(0-255)",
                       "MainColor B(0-255)",
                       "MainColor A(0-255)",
                       "ShadowColor R(0-255)",
                       "ShadowColor G(0-255)",
                       "ShadowColor B(0-255)",
                       "ShadowColor A(0-255)"));
            }
        }
    }
}

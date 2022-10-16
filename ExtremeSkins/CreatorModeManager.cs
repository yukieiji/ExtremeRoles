using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using BepInEx.Configuration;

using UnityEngine;

using ExtremeSkins.Module;

namespace ExtremeSkins
{
    public static class CreatorModeManager
    {
        public static bool IsEnable => creatorModeConfig.Value;

        private static ConfigEntry<bool> creatorModeConfig;

        private const string additionalTransCsv = "translation.csv";
        private const string additionalColorCsv = "color.csv";

        private const string folder = "CreatorWorkingDir";
        private const string slash = @"\";

        private const string comma = ",";

        public static void Initialize()
        {
            creatorModeConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "CreatorMode", false);

            if (IsEnable)
            {
                string creatorModePath = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    slash, folder);

                if (!Directory.Exists(creatorModePath))
                {
                    Directory.CreateDirectory(creatorModePath);
                }

                string colorCsvPath = string.Concat(
                    creatorModePath, slash, additionalColorCsv);
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

                string transCsv = string.Concat(
                    creatorModePath, slash, additionalTransCsv);
                if (File.Exists(transCsv))
                {

                }
            }
        }
    }
}

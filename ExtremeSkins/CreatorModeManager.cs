using System.IO;

using BepInEx.Configuration;

using UnityEngine;

namespace ExtremeSkins
{
    public static class CreatorModeManager
    {
        public static bool IsEnable => creatorModeConfig.Value;

        private static ConfigEntry<bool> creatorModeConfig;

        private const string folder = "CreatorWorkingDir";

        public static void Initialize()
        {
            creatorModeConfig = ExtremeSkinsPlugin.Instance.Config.Bind(
                "CreateNewSkin", "CreatorMode", false);

            if (IsEnable)
            {
                string creatorModePath = string.Concat(
                    Path.GetDirectoryName(Application.dataPath),
                    @"\", folder);

                if (!Directory.Exists(creatorModePath))
                {
                    Directory.CreateDirectory(creatorModePath);
                }
            }
        }
    }
}

using System.Linq;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using UnityEngine;

using HarmonyLib;
using ExtremeRoles.Module;


namespace ExtremeRoles
{

    [BepInAutoPlugin("me.yukieiji.extremeroles", "Extreme Roles")]
    [BepInProcess("Among Us.exe")]
    public partial class ExtremeRolesPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);

        public static ExtremeRolesPlugin Instance;
        public static GameDataContainer GameDataStore = new GameDataContainer();

        public static InfoOverlay Info = new InfoOverlay();

        internal static BepInEx.Logging.ManualLogSource Logger;
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {

            Helper.Translation.Load();

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "Enable Debug Mode", false);

            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(
                3, OptionHolder.VanillaMaxPlayerNum).ToArray(); // 最大インポスター数 = 推奨3人
            GameOptionsData.MinPlayers = Enumerable.Repeat(
                4, OptionHolder.VanillaMaxPlayerNum).ToArray(); // 最小プレイヤー数 = 4人

            Instance = this;

            OptionHolder.Create();
            OptionHolder.UpdateRegion();

            Harmony.PatchAll();
        }

    }
}

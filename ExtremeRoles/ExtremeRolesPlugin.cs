using System.Linq;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using HarmonyLib;


namespace ExtremeRoles
{

    [BepInAutoPlugin("me.yukieiji.extremeroles", "Extreme Roles")]
    [BepInProcess("Among Us.exe")]
    public partial class ExtremeRolesPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);

        public static ExtremeRolesPlugin Instance;
        public static Module.GameDataContainer GameDataStore = new Module.GameDataContainer();

        internal static BepInEx.Logging.ManualLogSource Logger;
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {

            Helper.Translation.Load();

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "Enable Debug Mode", false);

            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(
                3, OptionsHolder.VanillaMaxPlayerNum).ToArray(); // 最大インポスター数 = 推奨3人
            GameOptionsData.MinPlayers = Enumerable.Repeat(
                4, OptionsHolder.VanillaMaxPlayerNum).ToArray(); // 最小プレイヤー数 = 4人

            Instance = this;

            OptionsHolder.Create();
            OptionsHolder.UpdateRegion();

            Harmony.PatchAll();
        }

    }
}

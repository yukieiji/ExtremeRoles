using System.Linq;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using HarmonyLib;
using Reactor;

namespace ExtremeRoles
{

    [BepInAutoPlugin]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public partial class ExtremeRolesPlugin : BasePlugin
    {
        public Harmony harmony { get; } = new(Id);
        public static ExtremeRolesPlugin Instance;

        internal static BepInEx.Logging.ManualLogSource Logger;

        public static int OptionsPage = 1;

        public static ConfigEntry<bool> DebugMode { get; private set; }

        public static IRegionInfo[] DefaultRegions;
        public static void UpdateRegions()
        {
            ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
            IRegionInfo[] regions = DefaultRegions;

            var CustomRegion = new DnsRegionInfo(
                OptionsHolder.ConfigParser.Ip.Value, "Custom",
                StringNames.NoTranslation,
                OptionsHolder.ConfigParser.Ip.Value,
                OptionsHolder.ConfigParser.Port.Value);
            regions = regions.Concat(new IRegionInfo[] { CustomRegion.Cast<IRegionInfo>() }).ToArray();
            ServerManager.DefaultRegions = regions;
            serverManager.AvailableRegions = regions;
        }

        public override void Load()
        {

            Helper.Translation.Load();

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "Enable Debug Mode", false);
            DefaultRegions = ServerManager.DefaultRegions;

            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(
                3, OptionsHolder.VanillaMaxPlayerNum).ToArray(); // 最大インポスター数 = 推奨3人
            GameOptionsData.MinPlayers = Enumerable.Repeat(
                4, OptionsHolder.VanillaMaxPlayerNum).ToArray(); // 最小プレイヤー数 = 4人

            Instance = this;
            OptionsHolder.Create();

            UpdateRegions();

            harmony.PatchAll();
        }

    }
}

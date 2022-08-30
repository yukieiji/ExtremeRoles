using System.Linq;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using HarmonyLib;
using ExtremeRoles.Compat;
using ExtremeRoles.Module;
using ExtremeRoles.Module.InfoOverlay;


namespace ExtremeRoles
{

    [BepInAutoPlugin("me.yukieiji.extremeroles", "Extreme Roles")]
    [BepInDependency(
        ExtremeRoles.Compat.Mods.SubmergedMap.Guid,
        BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Among Us.exe")]
    public partial class ExtremeRolesPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);

        public static ExtremeRolesPlugin Instance;
        public static ExtremeShipStatus GameDataStore = new ExtremeShipStatus();

        public static InfoOverlay Info = new InfoOverlay();

        internal static BepInEx.Logging.ManualLogSource Logger;
        internal static CompatModManager Compat;
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {

            Helper.Translation.Load();

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "DebugMode", false);

            GameOptionsData.RecommendedImpostors = GameOptionsData.MaxImpostors = Enumerable.Repeat(
                3, OptionHolder.VanillaMaxPlayerNum).ToArray(); // 最大インポスター数 = 推奨3人
            GameOptionsData.MinPlayers = Enumerable.Repeat(
                4, OptionHolder.VanillaMaxPlayerNum).ToArray(); // 最小プレイヤー数 = 4人

            Instance = this;

            OptionHolder.Create();
            OptionHolder.UpdateRegion();

            Harmony.PatchAll();

            Compat = new CompatModManager();

            if (BepInExUpdater.UpdateRequired)
            {
                AddComponent<BepInExUpdater>();
            }

            Il2CppRegisterAttribute.Registration(
                System.Reflection.Assembly.GetAssembly(this.GetType()));
        
        }
    }
}

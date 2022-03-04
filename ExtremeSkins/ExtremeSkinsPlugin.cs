using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using HarmonyLib;


namespace ExtremeSkins
{

    [BepInAutoPlugin("me.yukieiji.extremeskins", "Extreme Skins")]
    [BepInDependency("me.yukieiji.extremeroles", BepInDependency.DependencyFlags.HardDependency)] // Never change it!
    [BepInProcess("Among Us.exe")]
    public partial class ExtremeSkinsPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);

        public static ExtremeSkinsPlugin Instance;

        internal static BepInEx.Logging.ManualLogSource Logger;
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "DebugMode", false);

            Instance = this;

            Harmony.PatchAll();
        }

    }
}

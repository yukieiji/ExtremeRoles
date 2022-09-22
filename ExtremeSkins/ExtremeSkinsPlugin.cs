using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;

using HarmonyLib;

using ExtremeSkins.SkinManager;


namespace ExtremeSkins
{

    [BepInAutoPlugin("me.yukieiji.extremeskins", "Extreme Skins")]
    [BepInDependency(
        ExtremeRoles.ExtremeRolesPlugin.Id,
        BepInDependency.DependencyFlags.HardDependency)] // Never change it!
    [BepInProcess("Among Us.exe")]
    public partial class ExtremeSkinsPlugin : BasePlugin
    {
        public Harmony Harmony { get; } = new Harmony(Id);

        public static ExtremeSkinsPlugin Instance;

        internal static BepInEx.Logging.ManualLogSource Logger;

        public static ConfigEntry<bool> CreatorMode { get; private set; }

        public const string SkinComitCategory = "SkinComit";

        public override void Load()
        {
            Logger = Log;
            CreatorMode = Config.Bind("CreateNewSkin", "CreatorMode", false);
            
            Instance = this;

#if WITHHAT
            ExtremeHatManager.Initialize();
#endif
#if WITHNAMEPLATE
            ExtremeNamePlateManager.Initialize();
#endif
#if WITHVISOR
            ExtremeVisorManager.Initialize();
#endif

            ExtremeColorManager.Initialize();

            VersionManager.PlayerVersion.Clear();

            Harmony.PatchAll();
        }

    }
}

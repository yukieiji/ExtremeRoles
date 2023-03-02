using System.Linq;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;

using HarmonyLib;
using ExtremeRoles.Compat;
using ExtremeRoles.Module;
using ExtremeRoles.Module.InfoOverlay;
using ExtremeRoles.Module.ExtremeShipStatus;
using ExtremeRoles.Resources;

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
        public static ExtremeShipStatus ShipState = new ExtremeShipStatus();

        public static InfoOverlay Info = new InfoOverlay();

        internal static BepInEx.Logging.ManualLogSource Logger;
        internal static CompatModManager Compat;
        public static ConfigEntry<bool> DebugMode { get; private set; }

        public override void Load()
        {

            Helper.Translation.Load();

            Logger = Log;

            DebugMode = Config.Bind("DeBug", "DebugMode", false);

            Instance = this;

            OptionHolder.Create();
            OptionHolder.UpdateRegion();

            Harmony.PatchAll();

            Compat = new CompatModManager();

            AddComponent<ExtremeRolePluginBehavior>();

            if (BepInExUpdater.UpdateRequired)
            {
                AddComponent<BepInExUpdater>();
            }

            Il2CppRegisterAttribute.Registration(
                System.Reflection.Assembly.GetAssembly(this.GetType()));

            Loader.LoadCommonAsset();
        }
    }
}

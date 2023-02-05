using System.Linq;

using UnityEngine;
using HarmonyLib;


using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    public static class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            // SliderInnner => GameGroup => Game Settings => PlayerOptionsMenu
            GameObject playerOptMenuObj = __instance.transform.parent.parent.parent.gameObject;

            if (playerOptMenuObj.GetComponent<ExtremeOptionMenu>() != null) { return; }

            // Adapt task count for main options
            modifiedDefaultGameOptions(__instance);

            playerOptMenuObj.AddComponent<ExtremeOptionMenu>();
        }

        private static void changeValueRange(
            UnhollowerBaseLib.Il2CppReferenceArray<OptionBehaviour> child,
            StringNames name, float minValue, float maxValue)
        {
            NumberOption numOpt = child.FirstOrDefault(x => x.Title == name)?.TryCast<NumberOption>();
            if (numOpt != null)
            {
                numOpt.ValidRange = new FloatRange(minValue, maxValue);
            }
        }

        private static void modifiedDefaultGameOptions(GameOptionsMenu instance)
        {
            UnhollowerBaseLib.Il2CppReferenceArray<OptionBehaviour> child = instance.Children;

            if (AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
                FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == "custom")
            {
                changeValueRange(child, StringNames.GameNumImpostors, 0f, GameSystem.MaxImposterNum);
            }

            changeValueRange(child, StringNames.GameCommonTasks, 0f, 4f );
            changeValueRange(child, StringNames.GameShortTasks , 0f, 23f);
            changeValueRange(child, StringNames.GameLongTasks  , 0f, 15f);
        }
    }
}

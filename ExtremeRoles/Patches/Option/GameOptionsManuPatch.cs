using System.Linq;

using UnityEngine;

using HarmonyLib;

using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Option
{
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    public static class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) { return; }

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
            NumberOption numOpt = child.FirstOrDefault(x => x.Title == name).TryCast<NumberOption>();
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
                changeValueRange(child, StringNames.GameNumImpostors, 0f, OptionHolder.MaxImposterNum);
            }

            changeValueRange(child, StringNames.GameCommonTasks, 0f, 4f );
            changeValueRange(child, StringNames.GameShortTasks , 0f, 23f);
            changeValueRange(child, StringNames.GameLongTasks  , 0f, 15f);
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public static class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;

        public static void Postfix(GameOptionsMenu __instance)
        {
            var gameSettingMenu = Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            AmongUsClient client = AmongUsClient.Instance;

            if (gameSettingMenu.RegularGameSettings.active || 
                gameSettingMenu.RolesSettings.gameObject.active ||
                !client) { return; }

            timer += Time.deltaTime;
            if (timer < 0.1f) { return; }
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;

            string name = __instance.name;
            bool isHost = client.AmHost;

            foreach (IOption option in OptionHolder.AllOption.Values)
            {
                if (!name.Equals($"{string.Format(
                        ExtremeOptionMenu.MenuNameTemplate, option.Tab.ToString())}_menu")) { continue; }


                if (option?.Body != null && option.Body.gameObject != null)
                {
                    bool enabled = option.IsActive() && isHost;

                    option.Body.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        bool isHeader = option.IsHeader;
                        offset -= isHeader ? 0.75f : 0.5f;
                        option.Body.transform.localPosition = new Vector3(
                            option.Body.transform.localPosition.x, offset,
                            option.Body.transform.localPosition.z);

                        if (isHeader)
                        {
                            numItems += 0.5f;
                        }
                    }
                    else
                    {
                        numItems--;
                    }
                }
            }
            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -4.0f + numItems * 0.5f;
        }
    }
}

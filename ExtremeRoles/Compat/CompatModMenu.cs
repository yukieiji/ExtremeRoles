using UnityEngine;
using UnityEngine.UI;

using UnhollowerBaseLib.Attributes;

using ExtremeRoles.Performance;

using BepInEx.IL2CPP.Utils.Collections;

namespace ExtremeRoles.Compat
{
    public class CompatModMenuPopUp : MonoBehaviour
    {
        [HideFromIl2Cpp]
        public void OnDisable()
        {
            base.gameObject.SetActive(false);
        }
    }

    internal static class CompatModMenu
    {
        private static GameObject menuBody;

        public static void CreateMenuButton(MainMenuManager instance)
        {
            GameObject buttonTemplate = GameObject.Find("AnnounceButton");
            GameObject compatModMenuButton = Object.Instantiate<GameObject>(
                buttonTemplate, buttonTemplate.transform.parent);
            compatModMenuButton.name = "CompatModMenuButton";
            compatModMenuButton.transform.SetSiblingIndex(7);
            PassiveButton compatModButton = compatModMenuButton.GetComponent<PassiveButton>();
            SpriteRenderer compatModSprite = compatModMenuButton.GetComponent<SpriteRenderer>();
            compatModButton.OnClick = new Button.ButtonClickedEvent();
            compatModButton.OnClick.AddListener((System.Action)(() =>
            {
                if (!menuBody)
                {
                    initMenu();
                }
                menuBody.gameObject.SetActive(true);
                
            }));
        }

        private static void initMenu()
        {
            menuBody = Object.Instantiate(
                FastDestroyableSingleton<EOSManager>.Instance.TimeOutPopup);
            menuBody.name = "ExtremeRoles_CompatModMenu";

            TMPro.TextMeshPro title = Object.Instantiate(
                Module.Prefab.Text, menuBody.transform);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.name = "title";
            title.text = "compatModMenu";
            title.transform.localPosition = new Vector3(0.0f, 2.45f, 0f);

            removeUnnecessaryComponent();
            setTransfoms();
            createCompatModLines();
        }

        private static void createCompatModLines()
        {

            var mods = new string[] {
                "0%", "10%", "20%", "30%", "40%",
                "50%", "60%", "70%", "80%", "90%", "100%" };

            int index = 0;

            foreach (string mod in mods)
            {
                TMPro.TextMeshPro modText = Object.Instantiate(
                    Module.Prefab.Text, menuBody.transform);
                modText.name = mod;

                modText.transform.localPosition = new Vector3(0.1f, 2.0f - (index * 0.35f), 0f);
                modText.fontSizeMin = modText.fontSizeMax = 3.0f;
                modText.font = Object.Instantiate(Module.Prefab.Text.font);
                modText.GetComponent<RectTransform>().sizeDelta = new Vector2(5.4f, 5.5f);
                modText.text = mod;
                modText.alignment = TMPro.TextAlignmentOptions.Left;
                modText.gameObject.SetActive(true);
                ++index;
            }
        }

        private static void removeUnnecessaryComponent()
        {
            var timeOutPopup = menuBody.GetComponent<TimeOutPopupHandler>();
            if (timeOutPopup != null)
            {
                Object.Destroy(timeOutPopup);
            }

            var controllerNav = menuBody.GetComponent<ControllerNavMenu>();
            if (controllerNav != null)
            {
                Object.Destroy(controllerNav);
            }

            Object.Destroy(menuBody.transform.FindChild("OfflineButton")?.gameObject);
            Object.Destroy(menuBody.transform.FindChild("RetryButton")?.gameObject);
            Object.Destroy(menuBody.transform.FindChild("Text_TMP")?.gameObject);
        }

        private static void setTransfoms()
        {
            Transform closeButtonTransform = menuBody.transform.FindChild("CloseButton");
            if (closeButtonTransform != null)
            {
                closeButtonTransform.localPosition = new Vector3(-3.25f, 2.5f, 0.0f);

                PassiveButton closeButton = closeButtonTransform.gameObject.GetComponent<PassiveButton>();
                closeButton.OnClick = new Button.ButtonClickedEvent();
                closeButton.OnClick.AddListener((System.Action)(() =>
                {
                    menuBody.gameObject.SetActive(false);

                }));
            }

            Transform bkSprite = menuBody.transform.FindChild("BackgroundSprite");
            if (bkSprite != null)
            {
                bkSprite.localScale = new Vector3(1.0f, 1.9f, 1.0f);
                bkSprite.localPosition = new Vector3(0.0f, 0.0f, 2.0f);
            }
        }

    }
}

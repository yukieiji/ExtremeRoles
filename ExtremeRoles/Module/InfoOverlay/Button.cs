using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.InfoOverlay
{
    public static class Button
    {
        public static GameObject Body = null;

        private static InfoOverlay.ShowType buttonShowType = 0;

        public static void CreateInfoButton()
        {
            Body = Object.Instantiate(
                GameObject.Find("MenuButton"),
                GameObject.Find("TopRight/MenuButton").transform);
            Object.DontDestroyOnLoad(Body);
            Body.name = "infoRoleButton";
            Body.gameObject.SetActive(true);
            Body.layer = 5;
            Body.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            SetInfoButtonToGameStartShipPositon();

            var passiveButton = Body.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityAction)toggleInfoOverlay);

            var render = Body.GetComponent<SpriteRenderer>();
            render.sprite = Loader.CreateSpriteFromResources(
                Path.HelpImage, 230f);
        }

        public static void SetInfoButtonToGameStartShipPositon()
        {
            Body.transform.localPosition = new Vector3(
                0.0f, -0.825f, 0.0f);
        }

        public static void SetInfoButtonToInGamePositon()
        {
            Body.gameObject.SetActive(true);
            Body.transform.localPosition = new Vector3(
                0.0f, -1.75f, 0.0f);
        }

        private static void toggleInfoOverlay()
        {
            if (ExtremeRolesPlugin.GameDataStore.IsRoleSetUpEnd)
            {
                show(InfoOverlay.ShowType.AllGhostRole);
            }
            else
            {
                show(InfoOverlay.ShowType.VanilaOption);
            }
        }

        private static void show(InfoOverlay.ShowType block)
        {
            if (buttonShowType >= block + 1)
            {
                ExtremeRolesPlugin.Info.HideInfoOverlay();
                buttonShowType = 0;
            }
            else
            {
                ExtremeRolesPlugin.Info.ToggleInfoOverlay(
                    buttonShowType);
                buttonShowType++;
            }
        }

    }
}

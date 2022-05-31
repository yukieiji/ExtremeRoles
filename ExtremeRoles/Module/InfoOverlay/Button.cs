using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.InfoOverlay
{
    public class Button
    {
        public static GameObject Body = null;

        public static void CreateInfoButton()
        {
            Body = Object.Instantiate(
                GameObject.Find("MenuButton"),
                GameObject.Find("TopRight").transform);
            Object.DontDestroyOnLoad(Body);
            Body.name = "infoRoleButton";
            Body.gameObject.SetActive(true);
            Body.layer = 5;

            SetInfoButtonToGameStartShipPositon();

            var passiveButton = Body.GetComponent<PassiveButton>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(
                (UnityAction)ExtremeRolesPlugin.Info.ToggleInfoOverlay);

            var render = Body.GetComponent<SpriteRenderer>();
            render.sprite = Loader.CreateSpriteFromResources(
                Path.HelpImage, 230f);
        }

        public static void SetInfoButtonToGameStartShipPositon()
        {
            Body.transform.localPosition = new Vector3(
                4.925f, 2.0f, 0.0f);
        }

        public static void SetInfoButtonToInGamePositon()
        {
            Body.gameObject.SetActive(true);
            Body.transform.localPosition = new Vector3(
                4.925f, 1.3f, 0.0f);
        }

    }
}

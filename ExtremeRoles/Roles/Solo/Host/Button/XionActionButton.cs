using System;
using UnityEngine;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Host.Button;

internal sealed class XionActionButton : NoneCoolButtonBase
{
    private string buttonText;

    public XionActionButton(
        Sprite sprite,
        Action buttonAction,
        string buttonText = "")
    {

        var hudManager = FastDestroyableSingleton<HudManager>.Instance;

        this.Body = UnityEngine.Object.Instantiate(
            hudManager.KillButton, hudManager.KillButton.transform.parent);
        PassiveButton button = this.Body.GetComponent<PassiveButton>();
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)OnClickEvent);

        this.ButtonAction = buttonAction;

        SetActive(false);

        var useButton = hudManager.UseButton;

        UnityEngine.Object.Destroy(
           this.Body.buttonLabelText.fontMaterial);
        this.Body.buttonLabelText.fontMaterial = UnityEngine.Object.Instantiate(
            useButton.buttonLabelText.fontMaterial, this.Body.transform);

        this.Body.graphic.sprite = sprite;
        this.Body.OverrideText(buttonText);

        this.buttonText = buttonText;

        ResetCoolTimer();
    }

    public override void Update()
    {
        if (this.Body)
        {
            this.Body.OverrideText(this.buttonText);
        }
        base.Update();
    }
}

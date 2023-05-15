using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class SimpleButton : MonoBehaviour
{
    public SpriteRenderer Image { get; private set; }
    public TextMeshPro Text { get; private set; }
    public Button.ButtonClickedEvent ClickedEvent { get; private set; }

    private Color defaultImgColor;
    private Color defaultTextColor;
    private Color overColor = Color.green;

    public void Awake()
    {
        this.Image = base.GetComponent<SpriteRenderer>();
        this.Text = base.GetComponentInChildren<TextMeshPro>();

        this.ClickedEvent = new Button.ButtonClickedEvent();

        this.defaultImgColor = this.Image.color;
        this.defaultTextColor = this.Text.color;
    }

    public void OnMouseDown()
    {
        if (this.ClickedEvent != null)
        {
            this.ClickedEvent.Invoke();
        }
    }

    public void OnMouseExit()
    {
        this.Image.color = defaultImgColor;
        this.Text.color = defaultTextColor;
    }

    public void OnMouseOver()
    {
        this.Text.color = overColor;
        this.Image.color = overColor;
    }
}

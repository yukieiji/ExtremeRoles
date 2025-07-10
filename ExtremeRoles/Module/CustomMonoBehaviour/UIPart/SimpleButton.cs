using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class SimpleButton : MonoBehaviour
{
	public int Layer
	{
		get => base.gameObject.layer;
		set
		{
			base.gameObject.layer = value;
			this.Text.gameObject.layer = value;
		}
	}

	public Vector3 Scale
	{
		get => base.transform.localScale;
		set
		{
			Vector3 prevTextScale = this.Text.transform.localScale;
			base.transform.localScale = value;
			this.Text.transform.localScale =
				new Vector3(
					prevTextScale.x / value.x,
					prevTextScale.y / value.y,
					prevTextScale.z / value.z);
		}
	}

	public SpriteRenderer Image { get; private set; }
	public TextMeshPro Text { get; private set; }
	public Button.ButtonClickedEvent ClickedEvent { get; set; }

	public Color DefaultImgColor { get; set; }
	public Color DefaultTextColor { get; set; }

	private Color overColor = Color.green;

	public SimpleButton(System.IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		this.Image = base.GetComponent<SpriteRenderer>();
		this.Text = base.GetComponentInChildren<TextMeshPro>();

		this.ClickedEvent = new Button.ButtonClickedEvent();

		this.DefaultImgColor = this.Image.color;
		this.DefaultTextColor = this.Text.color;
	}

	public void OnMouseDown()
	{
		this.Image.color = DefaultImgColor;
		this.Text.color = DefaultTextColor;

		if (this.ClickedEvent != null)
		{
			this.ClickedEvent.Invoke();
		}
	}

	public void OnMouseExit()
	{
		this.Image.color = DefaultImgColor;
		this.Text.color = DefaultTextColor;
	}

	public void OnMouseEnter()
	{
		this.Text.color = overColor;
		this.Image.color = overColor;
	}
}

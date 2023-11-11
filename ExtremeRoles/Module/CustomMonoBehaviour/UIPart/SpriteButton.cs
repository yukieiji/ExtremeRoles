using System;

using TMPro;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

#nullable enable

[Il2CppRegister]
public sealed class SpriteButton : MonoBehaviour
{
	public Action? OnClick { get; set; }


#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public TextMeshPro Text { get; private set; }
	public SpriteRenderer Rend { get; private set; }
	public BoxCollider2D Colider { get; private set; }

	public SpriteButton(IntPtr ptr) : base(ptr) {  }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public void Awake()
	{
		this.Rend = base.gameObject.AddComponent<SpriteRenderer>();
		this.Colider = base.gameObject.AddComponent<BoxCollider2D>();

		this.Text = Instantiate(Prefab.Text, base.transform);
		this.Text.alignment = TextAlignmentOptions.Center;
		this.Text.gameObject.SetActive(true);
		this.Text.transform.localPosition = new Vector3(0.0f, -1.0f);
		this.Text.fontSize = this.Text.fontSizeMin = this.Text.fontSizeMax = 3.0f;
	}

	public void OnMouseDown()
	{
		this.OnClick?.Invoke();
	}

	public void OnMouseEnter()
	{
		this.Rend.color = Color.green;
		this.Text.color = Color.green;
	}
	public void OnMouseExit()
	{
		this.Rend.color = Color.white;
		this.Text.color = Color.white;
	}
}

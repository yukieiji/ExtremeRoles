using System;

using TMPro;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class ExtremeOptionView(IntPtr ptr) : OptionBehaviour(ptr)
{
	private TextMeshPro? titleText;
	private TextMeshPro? valueText;

	[HideFromIl2Cpp]
	public IOption? OptionModel { private get; set; }

	[HideFromIl2Cpp]
	public OptionCategory? OptionCategoryModel { private get; set; }

	private readonly record struct AwakeProp(
		TextMeshPro Title,
		TextMeshPro Value,
		GameOptionButton Minus,
		GameOptionButton Plus,
		Transform ImgTrans);

	public void Awake()
	{
		var prop = base.TryGetComponent<StringOption>(out var opt) ?
			NomalAwake(opt) : DelayAwake();

		this.titleText = prop.Title;
		this.valueText = prop.Value;

		var curSizeDelt = this.titleText.rectTransform.sizeDelta;
		this.titleText.rectTransform.sizeDelta = new Vector2(4.25f, curSizeDelt.y);
		var curTextpos = this.titleText.transform.localPosition;
		this.titleText.transform.localPosition = new Vector3(-1.8f, curTextpos.y, curTextpos.z);

		prop.Minus.OnClick.RemoveAllListeners();
		prop.Minus.OnClick.AddListener(this.Decrease);

		prop.Plus.OnClick.RemoveAllListeners();
		prop.Plus.OnClick.AddListener(this.Increase);

		var imgTrans = prop.ImgTrans;
		var curPos = imgTrans.localPosition;
		imgTrans.localPosition = new Vector3(-1.915f, curPos.y, curPos.z);
		var curScale = imgTrans.localScale;
		imgTrans.localScale = new Vector3(1.5f, curScale.y, curScale.z);

		if (opt != null)
		{
			Destroy(opt);
		}
	}

	public void Decrease()
	{
		if (OptionModel is null ||
			OptionCategoryModel is null)
		{
			return;
		}
		OptionManager.Instance.UpdateToStep(OptionCategoryModel, OptionModel, -1);
	}
	public void Increase()
	{
		if (OptionModel is null ||
			OptionCategoryModel is null)
		{
			return;
		}
		OptionManager.Instance.UpdateToStep(OptionCategoryModel, OptionModel, 1);
	}

	public void SetMaterialLayer(int maskLayer)
	{
		var rends = base.GetComponentsInChildren<SpriteRenderer>(true);
		foreach (var rend in rends)
		{
			rend.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		}

		var textMeshPros = base.GetComponentsInChildren<TextMeshPro>(true);
		foreach (TextMeshPro textMeshPro in textMeshPros)
		{
			textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
			textMeshPro.fontMaterial.SetFloat("_Stencil", maskLayer);
		}
	}

	public void Refresh()
	{
		if (this.OptionModel is null)
		{
			return;
		}

		if (this.titleText != null)
		{
			this.titleText.text = this.OptionModel.Title;
		}
		if (this.valueText != null)
		{
			this.valueText.text = this.OptionModel.ValueString;
		}
	}

	private AwakeProp DelayAwake()
		=> new AwakeProp(
			base.transform.Find("Title Text").GetComponent<TextMeshPro>(),
			base.transform.Find("Value_TMP (1)").GetComponent<TextMeshPro>(),
			base.transform.Find("MinusButton").GetComponent<GameOptionButton>(),
			base.transform.Find("PlusButton").GetComponent<GameOptionButton>(),
			base.transform.Find("LabelBackground"));

	private AwakeProp NomalAwake(StringOption opt)
		=> new AwakeProp(
			opt.TitleText,
			opt.ValueText,
			opt.MinusBtn,
			opt.PlusBtn,
			opt.LabelBackground.transform);
}
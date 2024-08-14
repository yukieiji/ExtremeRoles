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

	public void Awake()
	{
		if (!base.TryGetComponent<StringOption>(out var opt))
		{
			return;
		}

		this.titleText = opt.TitleText;
		var curSizeDelt = this.titleText.rectTransform.sizeDelta;
		this.titleText.rectTransform.sizeDelta = new Vector2(4.25f, curSizeDelt.y);
		var curTextpos = this.titleText.transform.localPosition;
		this.titleText.transform.localPosition = new Vector3(-1.8f, curTextpos.y, curTextpos.z);

		this.valueText = opt.ValueText;

		opt.MinusBtn.OnClick.RemoveAllListeners();
		opt.MinusBtn.OnClick.AddListener(this.Decrease);

		opt.PlusBtn.OnClick.RemoveAllListeners();
		opt.PlusBtn.OnClick.AddListener(this.Increase);

		var imgTrans = opt.LabelBackground.transform;
		var curPos = imgTrans.localPosition;
		imgTrans.localPosition = new Vector3(-1.915f, curPos.y, curPos.z);
		var curScale = imgTrans.localScale;
		imgTrans.localScale = new Vector3(1.5f, curScale.y, curScale.z);


		Destroy(opt);
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
}
using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBodyBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	public readonly record struct ColorProperty(Color Active, Color Deactive, Color BodyColor);

	private Transform? bkTransform;
	private Transform? bodyTransform;
	private Image? body;
	private Image? backGround;
	private Action<bool>? act;

	private ColorProperty property = new ColorProperty(Color.green, Color.red, Color.white);
	private bool active = false;
	private const float offset = 0.5f;

	private Vector3 scale = Vector3.zero;


	[HideFromIl2Cpp]
	public void Initialize(ColorProperty color, bool isActive, Action<bool> act)
	{
		setUpObject();

		this.property = color;
		this.act = act;

		if (this.body != null)
		{
			this.body.color = this.property.BodyColor;
		}

		this.Set(isActive);
	}

	[HideFromIl2Cpp]
	private static EventTrigger.Entry createEventEntry(EventTriggerType trigger, Action<BaseEventData> action)
	{
		var callBack = new EventTrigger.TriggerEvent();
		callBack.AddListener((UnityAction<BaseEventData>)action);

		return new EventTrigger.Entry()
		{
			eventID = trigger,
			callback = callBack,
		};
	}

	public void Set(bool active)
	{
		float x = active ? offset : -offset;
		if (this.bodyTransform != null)
		{
			var curPos = this.bodyTransform.localPosition;
			this.bodyTransform.localPosition = new Vector3(x, curPos.y, curPos.z);
		}
		var color = active ? this.property.Active : this.property.Deactive;
		if (this.backGround != null)
		{
			this.backGround.color = color;
		}
		this.active = active;
		this.act?.Invoke(active);
	}

	private void setUpObject()
	{
		if (this.bkTransform == null)
		{
			this.bkTransform = base.transform.Find("Background");
		}

		if (this.bkTransform == null)
		{
			return;
		}

		if (this.backGround == null &&
			this.bkTransform.TryGetComponent<Image>(out var bkImage))
		{
			this.backGround = bkImage;
		}

		if (this.scale == Vector3.zero)
		{
			this.scale = this.transform.localScale;
		}

		if (!this.bkTransform.TryGetComponent<EventTrigger>(out _))
		{
			var trigger = this.bkTransform.gameObject.AddComponent<EventTrigger>();
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerClick, (_) => this.Set(!this.active)));
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerEnter, (_) => this.transform.localScale = this.scale * 1.05f));
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerExit, (_) => this.transform.localScale = this.scale));
		}

		if (this.bodyTransform == null)
		{
			this.bodyTransform = this.bkTransform.Find("ButtonBodyShadow");
		}


		if (this.bodyTransform == null ||
			this.body != null)
		{
			return;
		}

		// ボタンは影の子に本体があるので、影自体のオブジェクトから本体を探すで良い
		var bodyImgTrans = this.bodyTransform.Find("ButtonBody");
		if (bodyImgTrans != null &&
			this.bodyTransform.TryGetComponent<Image>(out var bodImg))
		{
			this.body = bodImg;
		}
	}
}
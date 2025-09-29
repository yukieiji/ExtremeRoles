using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBodyBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	public readonly record struct ColorProperty(Color Active, Color Deactive, Color BodyColor);

	private Transform? bodyTransform;
	private Image? body;
	private Image? backGround;
	private Action<bool>? act;

	private ColorProperty property = new ColorProperty(Color.green, Color.red, Color.white);
	private bool active = false;
	private float offset = 0.5f;

	private Vector3 size;

	public void Awake()
	{
		var bk = base.transform.Find("Background");
		if (bk == null)
		{
			return;
		}

		if (bk.TryGetComponent<Image>(out var bkImage))
		{
			this.backGround = bkImage;
		}
		this.size = this.transform.localScale;

		if (!bk.TryGetComponent<EventTrigger>(out _))
		{
			var trigger = bk.gameObject.AddComponent<EventTrigger>();
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerClick, (_) => this.Set(!this.active)));
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerEnter, (_) => this.transform.localScale = this.size * 1.05f));
			trigger.triggers.Add(createEventEntry(EventTriggerType.PointerExit, (_) => this.transform.localScale = this.size / 1.05f));
		}

		this.bodyTransform = bk.Find("ButtonBodyShadow");

		if (this.bodyTransform != null)
		{
			var bodyImgTrans = this.bodyTransform.Find("ButtonBody");
			if (bodyImgTrans != null &&
				bodyTransform.TryGetComponent<Image>(out var bodImg))
			{
				this.body = bodImg;
			}
		}
	}

	public void Initialize(ColorProperty color, bool isActive, Action<bool> act)
	{
		this.property = color;
		this.act = act;

		if (this.body != null)
		{
			this.body.color = this.property.BodyColor;
		}

		this.Set(isActive);
	}

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
		this.act?.Invoke(active);
		this.active = active;
	}
}
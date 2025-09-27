using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

[Il2CppRegister]
public sealed class ToggleButtonBodyBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
	public readonly struct ColorProperty
	{
		public readonly Color Active;
		public readonly Color Deactive;
		public readonly Color BodyColor;

		public ColorProperty(Color active, Color deactive, Color body)
		{
			this.Active = active;
			this.Deactive = deactive;
			this.BodyColor = body;
		}
	}

	private Transform bodyTransform;
	private Image body;
	private Image backGround;

	private ColorProperty property = new ColorProperty(Color.green, Color.red, Color.white);
	private bool active = false;
	private float offset = 0.5f;

	public void Awake()
	{
		var bk = base.transform.Find("Background");
		if (bk != null)
		{
			if (bk.TryGetComponent<Image>(out var bkImage))
			{
				this.backGround = bkImage;
			}
			var trigger = bk.gameObject.AddComponent<EventTrigger>();
			trigger.triggers.Add(onMouseClickEvent());
			trigger.triggers.Add(onMouseEnterEvent());
			trigger.triggers.Add(onMouseExitEvent());
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
		this.Set(false);
	}

	private EventTrigger.Entry onMouseClickEvent()
	{
		var callBack = new EventTrigger.TriggerEvent();
		callBack.AddListener((UnityAction<BaseEventData>)((_) => this.Set(!this.active)));

		return new EventTrigger.Entry()
		{
			eventID = EventTriggerType.PointerClick,
			callback = callBack,
		};
	}

	private EventTrigger.Entry onMouseEnterEvent()
	{
		var callBack = new EventTrigger.TriggerEvent();
		callBack.AddListener((UnityAction<BaseEventData>)((_) => this.transform.localScale *= 1.05f));

		return new EventTrigger.Entry()
		{
			eventID = EventTriggerType.PointerEnter,
			callback = callBack,
		};
	}

	private EventTrigger.Entry onMouseExitEvent()
	{
		var callBack = new EventTrigger.TriggerEvent();
		callBack.AddListener((UnityAction<BaseEventData>)((_) => this.transform.localScale /= 1.05f));

		return new EventTrigger.Entry()
		{
			eventID = EventTriggerType.PointerExit,
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
	}
}
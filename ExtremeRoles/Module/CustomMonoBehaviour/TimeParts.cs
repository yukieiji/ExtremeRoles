﻿using System;
using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister(
	new Type[]
	{
		typeof(IUsable)
	})]
public sealed class TimeParts : MonoBehaviour, IAmongUs.IUsable
{

	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.UseButton;
		}
	}

	public float UsableDistance
	{
		get
		{
			return 0.5f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0.0f;
		}
	}

	private bool used = false;
	private int id = 0;

	public TimeParts(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		this.used = false;

		var collider = base.gameObject.AddComponent<CircleCollider2D>();
		collider.radius = 0.1f;
		collider.isTrigger = true;

		var img = base.gameObject.AddComponent<SpriteRenderer>();

		img.sprite = Loader.CreateSpriteFromResources(
			Path.TheifTimeParts);
	}

	public void SetId(int id)
	{
		this.id = id;
	}

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = !this.used ? Vector2.Distance(
			pc.Object.GetTruePosition(),
			base.transform.position) : float.MaxValue;

		couldUse = pc.IsDead ? false : true;
		canUse = (couldUse && num <= this.UsableDistance);
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		this.used = true;
		ExtremeSystemTypeManager.RpcUpdateSystemOnlyHost(
			ExtremeSystemType.ThiefMeetingTimeChange,
			x =>
			{
				x.Write((byte)ThiefMeetingTimeStealSystem.Ops.PickUp);
				x.WritePacked(this.id);
			});
	}
}
using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister(
	new Type[]
	{
		typeof(IUsable)
	})]
public class PortalBase : MonoBehaviour
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

	private SpriteRenderer img;
	private Vector2 pos;
	private PortalBase linkPortal = null;
	private float timer = 0.0f;
	private const float NoneUsePortalTime = 5.0f;

	public PortalBase(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		var collider = base.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.1f;
        collider.isTrigger = true;

        this.img = base.gameObject.AddComponent<SpriteRenderer>();
		this.linkPortal = null;

		this.img.sprite = Loader.CreateSpriteFromResources(
            Path.TeleporterNoneActivatePortal);
    }

	public static void Link(PortalBase a, PortalBase b)
    {
		a.SetTarget(b.gameObject.transform.position);
        b.SetTarget(a.gameObject.transform.position);

		a.img.sprite = a.GetSprite();
		b.img.sprite = b.GetSprite();

		a.linkPortal = b;
		b.linkPortal = a;
    }

	public void SetTarget(Vector2 pos)
	{
		this.pos = pos;
	}

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = this.linkPortal && this.timer <= 0.0f ? 
			Vector2.Distance(
				pc.Object.GetTruePosition(),
				base.transform.position) : 
			float.MaxValue;

		couldUse = pc.IsDead ? false : true;
		canUse = (couldUse && num <= this.UsableDistance);
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		PlayerControl player = CachedPlayerControl.LocalPlayer;

        Player.RpcUncheckSnap(
            player.PlayerId, this.pos - player.Collider.offset);

		this.timer = NoneUsePortalTime;
		this.linkPortal.timer = NoneUsePortalTime;
    }

	public void FixedUpdate()
	{
		if (this.timer <= 0.0f) { return; }

        this.timer -= Time.fixedDeltaTime;
    }

	protected virtual Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.TestButton);
}

public sealed class PortalFirst : PortalBase
{
	public PortalFirst(IntPtr ptr) : base(ptr) { }

	protected override Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.TeleporterFirstPortal);
}

public sealed class PortalSecond : PortalBase
{
	public PortalSecond(IntPtr ptr) : base(ptr) { }

	protected override Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.TeleporterSecondPortal);
}
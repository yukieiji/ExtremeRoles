using System;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister(
   new Type[]
   {
		typeof(IUsable)
   })]
public sealed class TorchBehavior : MonoBehaviour, IAmongUs.IUsable
{
	public ImageNames UseIcon
	{
		get
		{
			return ImageNames.UseButton;
		}
	}

	public float UsableDistance { get; set; }

	public int GroupId { private get; set; }

	public float PercentCool
	{
		get
		{
			return 0.1f;
		}
	}

	private CircleCollider2D collider;
	private SpriteRenderer img;
	private Arrow arrow;

	public TorchBehavior(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		this.collider = base.gameObject.AddComponent<CircleCollider2D>();
		this.img = base.gameObject.AddComponent<SpriteRenderer>();
		this.img.sprite = Loader.CreateSpriteFromResources(
			Path.WispTorch);

		this.arrow = new Arrow(ColorPalette.KidsYellowGreen);
		this.arrow.SetActive(true);
		this.arrow.UpdateTarget(
			this.gameObject.transform.position);

		this.collider.isTrigger = true;
		this.collider.radius = 0.1f;


	}

	public void FixedUpdate()
	{
		this.arrow.SetActive(
			!MeetingHud.Instance && !ExileController.Instance);
	}

	public void OnDestroy()
	{
		this.arrow.Clear();
	}

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = Vector2.Distance(
			pc.Object.GetTruePosition(),
			base.transform.position);
		couldUse =
			!pc.IsDead &&
			ExtremeSystemTypeManager.Instance.TryGet<WispTorchSystem>(ExtremeSystemType.WispTorch, out var system) &&
			!system!.HasTorch(pc.PlayerId);
		canUse = (couldUse && num <= this.UsableDistance);
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.WispTorch,
			(writer) =>
			{
				writer.Write((byte)WispTorchSystem.Ops.PickUpTorch);
				writer.WritePacked(this.GroupId);
			});
	}
}

using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Crewmate;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister(
	new Type[]
	{
		typeof(IUsable)
	})]
public class TeleporterPortalPart : MonoBehaviour
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
			return 1.3f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0.1f;
		}
	}

	private SpriteRenderer img;
	private Teleporter teleporter;

	public TeleporterPortalBase(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		var collider = base.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.001f;
        
		var img = base.gameObject.AddComponent<SpriteRenderer>();
        img.sprite = Loader.CreateSpriteFromResources(
			Path.TestButton);
    }

	public void SetTeleporter(Teleporter teleporter)
	{
		this.teleporter = teleporter;
	}

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
		float num = Vector2.Distance(
			pc.Object.GetTruePosition(),
			base.transform.position);
		couldUse = pc.IsDead ? false : true;
		canUse = (couldUse && num <= this.UsableDistance);
		return num;
	}

	public void SetOutline(bool on, bool mainTarget)
	{ }

	public void Use()
	{
		this.teleporter.IncreaseAbilityCount();
    }
}
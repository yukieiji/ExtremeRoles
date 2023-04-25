using System;
using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Resources;
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

	public TeleporterPortalPart(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		var collider = base.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.001f;
        collider.isTrigger = true;

        var img = base.gameObject.AddComponent<SpriteRenderer>();
        img.sprite = Loader.CreateSpriteFromResources(
            Path.TeleporterPortalBase);
    }

	public float CanUse(
		GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
	{
        if (!tryGetTeleporter(out var _))
        {
			canUse = couldUse = false;
            return float.MaxValue;
        }

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
		if (!tryGetTeleporter(out Teleporter teleporter))
		{
			return;
		}
		teleporter.IncreaseAbilityCount();
        Destroy(base.gameObject);
    }
	
	private static bool tryGetTeleporter(out Teleporter teleporter)
	{
		teleporter = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Teleporter>();
		return teleporter != null;
	}
}
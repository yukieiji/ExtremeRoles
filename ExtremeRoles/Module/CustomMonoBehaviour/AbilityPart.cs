using System;
using System.Collections.Generic;
using UnityEngine;


using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using static ExtremeRoles.Roles.Solo.Impostor.Hypnotist;

using Il2CppInterop.Runtime.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

    [Il2CppRegister(
   new Type[]
   {
		typeof(IUsable)
   })]
    public class AbilityPartBase : MonoBehaviour
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
			return 0f;
		}
	}

	private SpriteRenderer img;
	private Arrow arrow;
	private float hideDistance;

	public AbilityPartBase(IntPtr ptr) : base(ptr) { }

	public void Awake()
	{
		var collider = base.gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.001f;

            this.img = base.gameObject.AddComponent<SpriteRenderer>();
            this.img.sprite = GetSprite();

            this.arrow = new Arrow(GetColor());
		this.arrow.SetActive(false);
		this.arrow.UpdateTarget(
			this.gameObject.transform.position);
	}

	public void SetHideArrowDistance(float newDistance)
        {
		this.hideDistance = newDistance;
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
		Hypnotist hypnotist = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Hypnotist>();
		Pickup(hypnotist);
		this.arrow.Clear();
		hypnotist.RemoveAbilityPartPos(
			base.gameObject.transform.position);
		Destroy(base.gameObject);
	}

	public void FixedUpdate()
        {
		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
		float distance = Vector2.Distance(
			localPlayer.GetTruePosition(),
			base.transform.position);

		bool isActive = 
			distance <= this.hideDistance && 
			!localPlayer.Data.IsDead &&
			!MeetingHud.Instance &&
			!ExileController.Instance;

		this.arrow.SetActive(isActive);
	}
	
	[HideFromIl2Cpp]
	protected virtual void Pickup(Hypnotist role)
        {

        }

	protected virtual Color GetColor() => Palette.ImpostorRed;

	protected virtual Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.TestButton);
}

public sealed class RedAbilityPart : AbilityPartBase
    {
	private const AbilityModuleType partType = AbilityModuleType.Red;

	public RedAbilityPart(IntPtr ptr) : base(ptr) { }

	[HideFromIl2Cpp]
	protected override void Pickup(Hypnotist hypnotist)
        {
		Helper.Logging.Debug("pickUp:RedPart");

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.HypnotistAbility))
		{
			caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)partType);
		}
		UpdateAllDollKillButtonState(hypnotist);
		hypnotist.EnableKillTimer();
	}

	protected override Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.HypnotistRedAbilityPart);
}

public sealed class BlueAbilityPart : AbilityPartBase
{
	private const AbilityModuleType partType = AbilityModuleType.Blue;
	private SystemConsoleType console;

	public BlueAbilityPart(IntPtr ptr) : base(ptr) { }

	public void SetConsoleType(SystemConsoleType console)
	{
		this.console = console;
	}

	[HideFromIl2Cpp]
	protected override void Pickup(Hypnotist hypnotist)
	{

		Helper.Logging.Debug($"pickUp:BluePart {console}");

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.HypnotistAbility))
		{
			caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)partType);
			caller.WriteByte((byte)this.console);
		}
		FeatAllDollMapModuleAccess(hypnotist, this.console);
	}
	protected override Color GetColor() => Palette.CrewmateBlue;

	protected override Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.HypnotistBlueAbilityPart);
}

public sealed class GrayAbilityPart : AbilityPartBase
{
	private const AbilityModuleType partType = AbilityModuleType.Glay;
	private SystemConsoleType console;

	public GrayAbilityPart(IntPtr ptr) : base(ptr) { }

	public void SetConsoleType(SystemConsoleType console)
        {
		this.console = console;
        }

	[HideFromIl2Cpp]
	protected override void Pickup(Hypnotist hypnotist)
	{
		Helper.Logging.Debug($"pickUp:GrayPart {console}");

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.HypnotistAbility))
		{
			caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)partType);
			caller.WriteByte((byte)this.console);
		}
		UnlockAllDollCrakingAbility(hypnotist, this.console);
	}

	protected override Color GetColor() => ColorPalette.NeutralColor;

	protected override Sprite GetSprite() => Loader.CreateSpriteFromResources(
		Path.HypnotistGrayAbilityPart);
}
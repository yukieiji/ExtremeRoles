using System;
using UnityEngine;


using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using static ExtremeRoles.Roles.Solo.Impostor.Hypnotist;

using Il2CppInterop.Runtime.Attributes;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister([ typeof(IUsable) ])]
public class AbilityPartBase : MonoBehaviour, IAmongUs.IUsable
{
	protected virtual AbilityModuleType PartType => AbilityModuleType.Red;

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
		collider.radius = 0.1f;

		this.img = base.gameObject.AddComponent<SpriteRenderer>();
		this.img.sprite = getSprite();

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
		NetworkedPlayerInfo pc, out bool canUse, out bool couldUse)
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
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
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

	private Sprite getSprite() => UnityObjectLoader.LoadFromResources(
		ExtremeRoleId.Hypnotist, PartType.ToString());
}

#pragma warning disable ERA002
public sealed class RedAbilityPart : AbilityPartBase
{
	protected override AbilityModuleType PartType => AbilityModuleType.Red;

	public RedAbilityPart(IntPtr ptr) : base(ptr) { }

	[HideFromIl2Cpp]
	protected override void Pickup(Hypnotist hypnotist)
	{
		Helper.Logging.Debug("pickUp:RedPart");

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.HypnotistAbility))
		{
			caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)PartType);
		}
		UpdateAllDollKillButtonState(hypnotist);
		hypnotist.EnableKillTimer();
	}
}

public sealed class BlueAbilityPart : AbilityPartBase
{
	protected override AbilityModuleType PartType => AbilityModuleType.Blue;
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
			caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)PartType);
			caller.WriteByte((byte)this.console);
		}
		FeatAllDollMapModuleAccess(hypnotist, this.console);
	}
	protected override Color GetColor() => Palette.CrewmateBlue;
}

public sealed class GrayAbilityPart : AbilityPartBase
{
	protected override AbilityModuleType PartType => AbilityModuleType.Gray;
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
			caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
			caller.WriteByte((byte)RpcOps.PickUpAbilityModule);
			caller.WriteByte((byte)PartType);
			caller.WriteByte((byte)this.console);
		}
		UnlockAllDollCrakingAbility(hypnotist, this.console);
	}

	protected override Color GetColor() => ColorPalette.NeutralColor;
}
#pragma warning restore ERA002
using System;
using System.Collections.Generic;
using UnityEngine;


using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using static ExtremeRoles.Roles.Solo.Impostor.Hypnotist;

using UnhollowerBaseLib.Attributes;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
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

		public AbilityPartBase(IntPtr ptr) : base(ptr) { }

		protected SpriteRenderer img;
		private CircleCollider2D collider;
		private Arrow arrow;
		private float hideDistance;

		public void Awake()
		{
			this.collider = base.gameObject.AddComponent<CircleCollider2D>();
			this.img = base.gameObject.AddComponent<SpriteRenderer>();

			this.img.material = new Material("HighlightMat");
			this.img.material.shader = Shader.Find("Sprite/Outline");

			this.collider.radius = 0.001f;
			this.arrow = new Arrow(GetColor());
			this.arrow.SetActive(false);
			this.arrow.UpdateTarget(
				this.gameObject.transform.position);
			this.img.sprite = Loader.CreateSpriteFromResources(
				Path.TestButton);
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
		{
			Color color = GetColor();
			this.img.material.SetFloat("_Outline", (float)(on ? 1 : 0));
			this.img.material.SetColor("_OutlineColor", color);
			this.img.material.SetColor("_AddColor", mainTarget ? color : Color.clear);
		}

		public void Use()
		{
			Hypnotist hypnotist = ExtremeRoleManager.GetSafeCastedLocalPlayerRole<Hypnotist>();
			Picup(hypnotist);
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

			this.arrow.SetActive(
				distance <= this.hideDistance && 
				!localPlayer.Data.IsDead);
		}

		protected virtual void Picup(Hypnotist role)
        {

        }
		protected virtual Color GetColor() => Palette.ImpostorRed;
    }

	public sealed class RedAbilityPart : AbilityPartBase
    {
		private const AbilityModuleType partType = AbilityModuleType.Red;

		public RedAbilityPart(IntPtr ptr) : base(ptr) { }

		[HideFromIl2Cpp]
		protected override void Picup(Hypnotist hypnotist)
        {
			PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

			RPCOperator.Call(
				rolePlayer.NetId,
				RPCOperator.Command.HypnotistAbility,
				new List<byte>
				{
					rolePlayer.PlayerId,
					(byte)RpcOps.PickUpAbilityModule,
					(byte)partType,
				});
			UpdateAllDollKillButtonState(hypnotist);
		}
    }

	public sealed class BlueAbilityPart : AbilityPartBase
	{
		private const AbilityModuleType partType = AbilityModuleType.Red;
		private SystemConsoleType console;

		public BlueAbilityPart(IntPtr ptr) : base(ptr) { }

		public void SetConsoleType(SystemConsoleType console)
		{
			this.console = console;
		}

		[HideFromIl2Cpp]
		protected override void Picup(Hypnotist hypnotist)
		{
			PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

			RPCOperator.Call(
				rolePlayer.NetId,
				RPCOperator.Command.HypnotistAbility,
				new List<byte>
				{
					rolePlayer.PlayerId,
					(byte)RpcOps.PickUpAbilityModule,
					(byte)partType,
					(byte)this.console,
				});
			FeatAllDollMapModuleAccess(hypnotist, this.console);
		}
		protected override Color GetColor() => Palette.CrewmateBlue;
	}

	public sealed class GrayAbilityPart : AbilityPartBase
	{
		private const AbilityModuleType partType = AbilityModuleType.Red;
		private SystemConsoleType console;

		public GrayAbilityPart(IntPtr ptr) : base(ptr) { }

		public void SetConsoleType(SystemConsoleType console)
        {
			this.console = console;
        }

		protected override void Picup(Hypnotist hypnotist)
		{
			PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

			RPCOperator.Call(
				rolePlayer.NetId,
				RPCOperator.Command.HypnotistAbility,
				new List<byte>
				{
					rolePlayer.PlayerId,
					(byte)RpcOps.PickUpAbilityModule,
					(byte)partType,
					(byte)this.console,
				});
			UnlockAllDollCrakingAbility(hypnotist, this.console);
		}

		protected override Color GetColor() => Palette.DisabledGrey;
	}
}
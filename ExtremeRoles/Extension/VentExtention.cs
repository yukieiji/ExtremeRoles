using System;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;


#nullable enable

namespace ExtremeRoles.Extension.VentModule;

public static class VentExtention
{
	public static bool IsModed(this Vent? vent)
		=> vent != null && CustomVent.IsExist && CustomVent.Instance.Contains(vent.Id);

	public static void PlayVentAnimation(this Vent? vent)
	{
		var hud = HudManager.Instance;
		if (hud == null ||
			vent == null ||
			!vent.IsCanAnimate())
		{
			return;
		}

		if (vent.IsModed() && vent.myRend != null)
		{
			int ventId = vent.Id;

			hud.StartCoroutine(
				Effects.Lerp(
					0.6f, new Action<float>((p) =>
					{

						int selector = p != 1f ? (int)(p * 17) : 0;

						vent.myRend.sprite = CustomVent.Instance.GetSprite(ventId, selector);

					})
				)
			);
		}
		else if (vent.myAnim != null)
		{
			vent.myAnim.Play(vent.ExitVentAnim, 1f);
		}
	}

	public static bool IsCanAnimate(this Vent vent)
	{
		PlayerControl? localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			return true;
		}

		var ventPos = vent.transform.position;
		var playerPos = localPlayer.transform.position;

		switch (ExtremeGameModeManager.Instance.ShipOption.Vent.AnimationMode)
		{
			case VentAnimationMode.DonotWallHack:
				return !PhysicsHelpers.AnythingBetween(
					localPlayer.Collider, playerPos, ventPos,
					Constants.ShipOnlyMask, false);
			case VentAnimationMode.DonotOutVison:

				// 視界端ギリギリが見えないのは困るのでライトオフセットとか言う値で調整しておく
				float distance = Vector2.Distance(playerPos, ventPos) - 0.18f;

				return !PhysicsHelpers.AnythingBetween(
					localPlayer.Collider, playerPos, ventPos,
					Constants.ShipOnlyMask, false) &&
					localPlayer.lightSource.viewDistance >= distance;
			default:
				return true;
		};
	}
}

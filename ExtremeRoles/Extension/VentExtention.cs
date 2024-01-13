using System;

using ExtremeRoles.Module;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Extension.VentModule;

public static class VentExtention
{
	public static bool IsModed(this Vent? vent)
		=> vent != null && CustomVent.IsExist && CustomVent.Instance.Contains(vent.Id);

	public static void PlayVentAnimation(this Vent? vent)
	{
		var hud = FastDestroyableSingleton<HudManager>.Instance;
		if (hud == null || vent == null) { return; }

		if (vent.IsModed() || vent.myRend != null)
		{
			int ventId = vent.Id;

			hud.StartCoroutine(
				Effects.Lerp(
					0.6f, new Action<float>((p) => {

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
}

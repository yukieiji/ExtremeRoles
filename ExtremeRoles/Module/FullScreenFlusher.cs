using System.Collections;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class FullScreenFlusherWithAudio
{
	private readonly AudioClip? audio;
	private readonly WaitForSeconds waiter;
	private readonly WaitForSeconds defaultWaiter = new WaitForSeconds(1.0f);
	private readonly Color screenColor;

	private Coroutine? coroutine;
	private SpriteRenderer? flush;

	private static HudManager hud => FastDestroyableSingleton<HudManager>.Instance;

	public FullScreenFlusherWithAudio(
		in AudioClip? audio, Color color,
		in float seconds = 1.0f)
	{
		this.audio = audio;
		this.screenColor = color;
		this.waiter = new WaitForSeconds(seconds);
	}
	public void SetActive(in bool enable)
	{
		if (hud == null) { return; }

		bool flashIsNull = this.coroutine == null;

		if (enable && flashIsNull)
		{
			this.coroutine = hud.StartCoroutine(
				startReactorFlush());
		}
		else if (!(enable || flashIsNull))
		{
			hud.StopCoroutine(this.coroutine);
			this.coroutine = null;
			if (this.flush != null)
			{
				this.flush.gameObject.SetActive(false);
				this.flush.enabled = false;
			}
		}
	}

	private IEnumerator startReactorFlush()
	{
		if (this.flush == null)
		{
			var hudMng = hud;
			this.flush = Object.Instantiate(
				 hudMng.FullScreen,
				 hudMng.transform);
			this.flush.transform.localPosition = new Vector3(0f, 0f, 20f);
			this.flush.color = this.screenColor;
		}

		this.flush.gameObject.SetActive(true);

		while (true)
		{
			this.flush.enabled = !this.flush.enabled;
			SoundManager.Instance.PlaySound(this.audio, false, 1f, null);

			yield return this.flush.enabled ? this.defaultWaiter : this.waiter;
		}
	}
}

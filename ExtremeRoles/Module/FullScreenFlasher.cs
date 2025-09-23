using System;
using System.Collections;

using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class FullScreenFlasher
{
	private readonly struct TimeLineInfo(float fadeInTime, float holdTime, float fadeOutTime)
	{
		public readonly float FadeIn = fadeInTime;
		public readonly float Hold = fadeInTime + holdTime;
		public readonly float FadeOutLength = fadeOutTime;
		public readonly float Total = fadeInTime + holdTime + fadeOutTime;
	}
	private SpriteRenderer? renderer;
	private readonly Color defaultColor;
	private readonly float maxAlpha;
	private readonly TimeLineInfo timeLine;
	private readonly Action<float> defaultLerpAction;


	public FullScreenFlasher(Color defaultColor, float maxAlpha=1.5f, float fadeInTime=0.5f, float fadeOutTime=0.5f, float holdTime = 0.0f)
	{
		if (fadeInTime <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(fadeInTime), "must be positive.");
		}
		if (fadeOutTime <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(fadeOutTime), "must be positive.");
		}
		if (holdTime < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(holdTime), "cannot be negative.");
		}

		this.defaultColor = defaultColor;
		this.maxAlpha = Mathf.Clamp01(maxAlpha);
		this.timeLine = new TimeLineInfo(fadeInTime, holdTime, fadeOutTime);
		this.defaultLerpAction = createLerpAction(this.defaultColor);
	}

	public void Flash(Color? overrideColor = null)
	{
		var hudManager = HudManager.Instance;
		if (hudManager == null)
		{
			return;
		}

		if (renderer == null)
		{
			renderer = UnityObject.Instantiate(hudManager.FullScreen, hudManager.transform);
			renderer.transform.localPosition = new Vector3(0f, 0f, 20f);
		}

		renderer.gameObject.SetActive(true);
		renderer.enabled = true;
		var colorToUse = overrideColor ?? defaultColor;
		renderer.color = new Color(colorToUse.r, colorToUse.g, colorToUse.b, 0f);

		var actionToRun = overrideColor.HasValue
			? createLerpAction(overrideColor.Value)
			: this.defaultLerpAction;

		hudManager.StartCoroutine(Effects.Lerp(this.timeLine.Total, actionToRun));
	}

	public void Hide()
	{
		if (this.renderer != null)
		{
			this.renderer.enabled = false;
		}
	}

	public void Reset()
	{
		if (this.renderer != null)
		{
			UnityObject.Destroy(this.renderer.gameObject);
		}
	}

	private Action<float> createLerpAction(Color color)
	{
		return (p) =>
		{
			if (renderer == null)
			{
				return;
			}

			float elapsed = p * this.timeLine.Total;
			float alpha = 0f;

			if (elapsed < this.timeLine.FadeIn)
			{
				alpha = (elapsed / this.timeLine.FadeIn) * this.maxAlpha;
			}
			else if (elapsed < this.timeLine.Hold)
			{
				alpha = this.maxAlpha;
			}
			else
			{
				float fadeOutElapsed = elapsed - this.timeLine.Hold;
				alpha = (1f - (fadeOutElapsed / this.timeLine.FadeOutLength)) * this.maxAlpha;
			}

			this.renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));

			if (p >= 1)
			{
				this.renderer.enabled = false;
			}
		};
	}
}

public sealed class FullScreenRepeatFlasherWithAudio
{
	private readonly AudioClip? audio;
	private readonly WaitForSeconds waiter;
	private readonly WaitForSeconds defaultWaiter = new WaitForSeconds(1.0f);
	private readonly Color screenColor;

	private Coroutine? coroutine;
	private SpriteRenderer? flush;

	private static HudManager hud => HudManager.Instance;

	public FullScreenRepeatFlasherWithAudio(
		AudioClip? audio, Color color,
		float seconds = 1.0f)
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
			this.flush = UnityObject.Instantiate(
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

using System;

using UnityEngine;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class FlasherScreenEffect
{
	private readonly struct TimeLineInfo(float fadeInTime, float holdTime, float maxShakeAmount)
	{
		public readonly float FadeIn = fadeInTime;
		public readonly float Hold = fadeInTime + holdTime;
		public readonly float MaxShakeAmount = maxShakeAmount;
	}

	private const float fixedFadeInTime = 0.01f;

	private SpriteRenderer? renderer;
	private Coroutine? activeCoroutine;
	private readonly Color defaultColor;
	private readonly float maxAlpha = 1.0f;
	private readonly TimeLineInfo timeLine;

	public FlasherScreenEffect(Color defaultColor, float holdTime, float maxShakeAmount)
	{
		if (holdTime <= 0.0f)
		{
			throw new ArgumentOutOfRangeException(nameof(holdTime), "must be positive.");
		}

		this.defaultColor = defaultColor;
		this.timeLine = new TimeLineInfo(fixedFadeInTime, holdTime, maxShakeAmount);
	}

	public void Flash(float fadeOutTime)
	{
		if (fadeOutTime <= 0.0f)
		{
			fadeOutTime = 0.01f;
		}

		var hudManager = HudManager.Instance;
		if (hudManager == null)
		{
			return;
		}

		if (this.renderer == null)
		{
			this.renderer = UnityObject.Instantiate(hudManager.FullScreen, hudManager.transform);
			this.renderer.transform.localPosition = new Vector3(0f, 0f, 20f);
		}

		this.renderer.gameObject.SetActive(true);
		this.renderer.enabled = true;
		this.renderer.color = new Color(this.defaultColor.r, this.defaultColor.g, this.defaultColor.b, 0f);

		if (this.activeCoroutine != null)
		{
			hudManager.StopCoroutine(this.activeCoroutine);
			this.activeCoroutine = null;
		}

		FollowerCamera? followerCamera = getFollowerCamera(hudManager);

		float totalTime = this.timeLine.Hold + fadeOutTime;

		Action<float> lerpAction = (p) =>
		{
			if (this.renderer == null)
			{
				return;
			}

			float elapsed = p * totalTime;
			float alpha = 0f;
			float currentShake = 0f;

			if (elapsed < this.timeLine.FadeIn)
			{
				alpha = (elapsed / this.timeLine.FadeIn) * this.maxAlpha;
				currentShake = this.timeLine.MaxShakeAmount;
			}
			else if (elapsed < this.timeLine.Hold)
			{
				alpha = this.maxAlpha;
				currentShake = this.timeLine.MaxShakeAmount;
			}
			else
			{
				float fadeOutElapsed = elapsed - this.timeLine.Hold;
				float fadeOutProgress = 1.0f - (fadeOutElapsed / fadeOutTime);
				alpha = fadeOutProgress * this.maxAlpha;
				currentShake = fadeOutProgress * this.timeLine.MaxShakeAmount;
			}

			this.renderer.color = new Color(this.defaultColor.r, this.defaultColor.g, this.defaultColor.b, Mathf.Clamp01(alpha));

			if (followerCamera != null)
			{
				followerCamera.shakeAmount = Mathf.Max(0f, currentShake);
			}

			if (p >= 1.0f)
			{
				this.renderer.enabled = false;
				if (followerCamera != null)
				{
					followerCamera.shakeAmount = 0f;
				}
			}
		};

		this.activeCoroutine = hudManager.StartCoroutine(Effects.Lerp(totalTime, lerpAction));
	}

	public void Hide()
	{
		var hudManager = HudManager.Instance;
		if (hudManager != null && this.activeCoroutine != null)
		{
			hudManager.StopCoroutine(this.activeCoroutine);
			this.activeCoroutine = null;
		}

		if (this.renderer != null)
		{
			this.renderer.enabled = false;
		}

		FollowerCamera? followerCamera = getFollowerCamera(hudManager);
		if (followerCamera != null)
		{
			followerCamera.shakeAmount = 0f;
		}
	}

	public void Reset()
	{
		Hide();
		if (this.renderer != null)
		{
			UnityObject.Destroy(this.renderer.gameObject);
			this.renderer = null;
		}
	}

	private static FollowerCamera? getFollowerCamera(HudManager? hudManager)
	{
		if (hudManager != null &&
			hudManager.transform != null &&
			hudManager.transform.parent != null)
		{
			return hudManager.transform.parent.GetComponent<FollowerCamera>();
		}

		return null;
	}
}

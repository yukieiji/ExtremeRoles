using System;

using UnityEngine;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class FlasherScreenEffect
{
	private readonly struct TimeLineInfo(float fadeInTime, float holdTime, float fadeOutTime)
	{
		public readonly float FadeIn = fadeInTime;
		public readonly float Hold = fadeInTime + holdTime;
		public readonly float FadeOutLength = fadeOutTime;
		public readonly float Total = fadeInTime + holdTime + fadeOutTime;
	}

	private SpriteRenderer? renderer;
	private Coroutine? activeCoroutine;
	private readonly Color defaultColor = Color.white;
	private readonly float maxAlpha = 1.0f;
	private readonly TimeLineInfo timeLine;
	private readonly float maxShakeAmount;

	public FlasherScreenEffect(float fadeInTime, float holdTime, float fadeOutTime, float maxShakeAmount)
	{
		if (fadeInTime <= 0.0f)
		{
			fadeInTime = 0.01f;
		}
		if (fadeOutTime <= 0.0f)
		{
			fadeOutTime = 0.01f;
		}
		if (holdTime < 0.0f)
		{
			holdTime = 0.0f;
		}

		this.timeLine = new TimeLineInfo(fadeInTime, holdTime, fadeOutTime);
		this.maxShakeAmount = maxShakeAmount;
	}

	public void Flash()
	{
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

		Action<float> lerpAction = (p) =>
		{
			if (this.renderer == null)
			{
				return;
			}

			float elapsed = p * this.timeLine.Total;
			float alpha = 0f;
			float currentShake = 0f;

			if (elapsed < this.timeLine.FadeIn)
			{
				alpha = (elapsed / this.timeLine.FadeIn) * this.maxAlpha;
				currentShake = this.maxShakeAmount;
			}
			else if (elapsed < this.timeLine.Hold)
			{
				alpha = this.maxAlpha;
				currentShake = this.maxShakeAmount;
			}
			else
			{
				float fadeOutElapsed = elapsed - this.timeLine.Hold;
				float fadeOutProgress = 1.0f - (fadeOutElapsed / this.timeLine.FadeOutLength);
				alpha = fadeOutProgress * this.maxAlpha;
				currentShake = fadeOutProgress * this.maxShakeAmount;
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

		this.activeCoroutine = hudManager.StartCoroutine(Effects.Lerp(this.timeLine.Total, lerpAction));
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
		FollowerCamera? followerCamera = null;
		try
		{
			if (Camera.main != null)
			{
				followerCamera = Camera.main.GetComponent<FollowerCamera>();
			}
		}
		catch
		{
		}

		if (followerCamera == null &&
			hudManager != null &&
			hudManager.transform != null &&
			hudManager.transform.parent != null)
		{
			followerCamera = hudManager.transform.parent.GetComponent<FollowerCamera>();
		}

		return followerCamera;
	}
}

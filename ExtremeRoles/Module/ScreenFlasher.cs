using System;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module
{
    public class ScreenFlasher
    {
        private SpriteRenderer? renderer;
        private readonly Color defaultColor;
        private readonly float fadeInTime;
        private readonly float holdTime;
        private readonly float fadeOutTime;
        private readonly float maxAlpha;
        private readonly float totalDuration;
        private readonly Action<float> defaultLerpAction;

        public ScreenFlasher(Color defaultColor, float maxAlpha, float fadeInTime, float fadeOutTime, float holdTime = 0.0f)
        {
            if (fadeInTime <= 0) throw new ArgumentOutOfRangeException(nameof(fadeInTime), "must be positive.");
            if (fadeOutTime <= 0) throw new ArgumentOutOfRangeException(nameof(fadeOutTime), "must be positive.");
            if (holdTime < 0) throw new ArgumentOutOfRangeException(nameof(holdTime), "cannot be negative.");

            this.defaultColor = defaultColor;
            this.maxAlpha = Mathf.Clamp01(maxAlpha);
            this.fadeInTime = fadeInTime;
            this.holdTime = holdTime;
            this.fadeOutTime = fadeOutTime;
            this.totalDuration = fadeInTime + holdTime + fadeOutTime;
            this.defaultLerpAction = CreateLerpAction(this.defaultColor);
        }

        private Action<float> CreateLerpAction(Color color)
        {
            return (p) =>
            {
                if (renderer == null)
                {
                    return;
                }

                float elapsed = p * this.totalDuration;
                float alpha = 0f;

                if (elapsed < this.fadeInTime)
                {
                    alpha = (elapsed / this.fadeInTime) * this.maxAlpha;
                }
                else if (elapsed < this.fadeInTime + this.holdTime)
                {
                    alpha = this.maxAlpha;
                }
                else
                {
                    float fadeOutElapsed = elapsed - (this.fadeInTime + this.holdTime);
                    alpha = (1f - (fadeOutElapsed / this.fadeOutTime)) * this.maxAlpha;
                }

                renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));

                if (p >= 1.0f)
                {
                    renderer.enabled = false;
                }
            };
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
                renderer = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
                renderer.transform.localPosition = new Vector3(0f, 0f, 20f);
            }

            renderer.gameObject.SetActive(true);
            renderer.enabled = true;
            renderer.color = new Color((overrideColor ?? defaultColor).r, (overrideColor ?? defaultColor).g, (overrideColor ?? defaultColor).b, 0f);

            if (this.totalDuration <= 0f)
            {
                if(renderer != null)
                {
                    renderer.enabled = false;
                }
                return;
            }

            Action<float> actionToRun = overrideColor.HasValue
                ? CreateLerpAction(overrideColor.Value)
                : this.defaultLerpAction;

            hudManager.StartCoroutine(Effects.Lerp(this.totalDuration, actionToRun));
        }
    }
}

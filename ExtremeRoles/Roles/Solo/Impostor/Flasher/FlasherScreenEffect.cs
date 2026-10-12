using System;
using System.Collections;

using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils.Collections;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Flasher;

/// <summary>
/// フラッシュによる視界奪取の演出を制御する。
/// </summary>
public interface IFlashScreenEffect
{
	bool IsRunning { get; }

	/// <summary>
	/// 画面の塗りつぶしを開始する。
	/// </summary>
	/// <param name="maxHoldSeconds">RPCが届かなかった場合に自己終了するまでの時間</param>
	/// <param name="color">画面を塗りつぶす色</param>
	void StartHold(float maxHoldSeconds, Color color);

	/// <summary>
	/// 画面の塗りつぶしから余韻へ移行する。
	/// </summary>
	void StartAfterglow(float afterglowTime);

	/// <summary>
	/// 演出を完全に停止する。
	/// </summary>
	void Stop();
}

/// <summary>
/// フラッシュ演出の時間軸を計算するだけのクラス。
/// </summary>
public static class FlasherScreenTimeline
{
	/// <summary>画面の塗りつぶし保持中の透明度</summary>
	public const float HoldAlpha = 1.0f;

	/// <summary>余韻の開始時に残る塗りつぶしの強さ</summary>
	public const float AfterglowStartAlpha = 0.6f;

	/// <summary>余韻開始時のシェイクの最大振幅</summary>
	public const float MaxShakeDistance = 0.12f;

	/// <summary>シェイクの振動数</summary>
	private const float ShakeFrequency = 45.0f;

	/// <summary>
	/// 余韻が終了したかどうかを返す。
	/// </summary>
	public static bool IsAfterglowFinished(float elapsed, float afterglowTime)
		=> afterglowTime <= 0.0f || elapsed >= afterglowTime;

	/// <summary>
	/// 余韻の経過時間に対応する塗りつぶしの透明度を返す。
	/// </summary>
	public static float GetAfterglowAlpha(float elapsed, float afterglowTime)
	{
		if (IsAfterglowFinished(elapsed, afterglowTime))
		{
			return 0.0f;
		}

		float decay = 1.0f - (elapsed / afterglowTime);
		return AfterglowStartAlpha * decay;
	}

	/// <summary>
	/// 余韻の経過時間に対応するカメラの揺れ幅を返す。
	/// </summary>
	public static (float X, float Y) GetAfterglowShakeOffset(
		float elapsed, float afterglowTime)
	{
		if (IsAfterglowFinished(elapsed, afterglowTime))
		{
			return (0.0f, 0.0f);
		}

		float decay = 1.0f - (elapsed / afterglowTime);
		float angle = elapsed * ShakeFrequency;
		float distance = MaxShakeDistance * decay;

		// Unityの数学APIを使わないことで、演出の時間軸をテストから検証できるようにする
		return (
			(float)Math.Cos(angle) * distance,
			(float)Math.Sin(angle) * distance);
	}
}

/// <summary>
/// 画面全体を覆う塗りつぶしと、余韻としてのカメラシェイクを実装する。
/// </summary>
public sealed class FlasherScreenEffect : IFlashScreenEffect
{
	private enum Phase
	{
		None,
		Hold,
		Afterglow,
	}

	private Phase phase = Phase.None;
	private float elapsed;
	private float phaseLimit;
	private bool isShaking;
	private float appliedShakeX;
	private float appliedShakeY;
	private Color overlayColor = Color.white;

	private SpriteRenderer? overlay;
	private Coroutine? coroutine;

	public bool IsRunning => this.phase != Phase.None;

	public void StartHold(float maxHoldSeconds, Color color)
	{
		if (createOverlay() == null)
		{
			return;
		}

		this.phase = Phase.Hold;
		this.elapsed = 0.0f;
		this.phaseLimit = maxHoldSeconds;
		this.overlayColor = color;

		setOverlayAlpha(FlasherScreenTimeline.HoldAlpha);
		startRoutine();
	}

	public void StartAfterglow(float afterglowTime)
	{
		if (this.phase == Phase.None)
		{
			return;
		}

		if (afterglowTime <= 0.0f)
		{
			Stop();
			return;
		}

		this.phase = Phase.Afterglow;
		this.elapsed = 0.0f;
		this.phaseLimit = afterglowTime;
	}

	public void Stop()
	{
		this.phase = Phase.None;
		this.elapsed = 0.0f;
		this.phaseLimit = 0.0f;

		stopRoutine();
		resetShake();
		setOverlayAlpha(0.0f);
	}

	private IEnumerator routine()
	{
		while (true)
		{
			yield return null;

			if (this.phase == Phase.None)
			{
				yield break;
			}

			this.elapsed += Time.deltaTime;

			if (this.phase == Phase.Hold)
			{
				// RPCが届かない場合に備えて時間切れで閉じる
				if (this.elapsed >= this.phaseLimit)
				{
					Stop();
					yield break;
				}
				continue;
			}

			if (FlasherScreenTimeline.IsAfterglowFinished(this.elapsed, this.phaseLimit))
			{
				Stop();
				yield break;
			}

			updateAfterglow();
		}
	}

	private void updateAfterglow()
	{
		setOverlayAlpha(
			FlasherScreenTimeline.GetAfterglowAlpha(this.elapsed, this.phaseLimit));

		(float X, float Y) offset =
			FlasherScreenTimeline.GetAfterglowShakeOffset(this.elapsed, this.phaseLimit);
		applyShake(offset.X, offset.Y);
		this.isShaking = true;
	}

	private SpriteRenderer? createOverlay()
	{
		if (this.overlay != null)
		{
			this.overlay.gameObject.SetActive(true);
			this.overlay.enabled = true;
			return this.overlay;
		}

		var hudManager = HudManager.Instance;
		if (hudManager == null)
		{
			return null;
		}

		this.overlay = UnityObject.Instantiate(hudManager.FullScreen, hudManager.transform);
		this.overlay.transform.localPosition = new Vector3(0f, 0f, 20f);
		this.overlay.gameObject.SetActive(true);
		this.overlay.enabled = true;

		return this.overlay;
	}

	private void setOverlayAlpha(float alpha)
	{
		if (this.overlay == null)
		{
			return;
		}

		this.overlay.color = new Color(
			this.overlayColor.r, this.overlayColor.g, this.overlayColor.b, alpha);
		this.overlay.enabled = alpha > 0.0f;
	}

	private void startRoutine()
	{
		if (this.coroutine != null)
		{
			return;
		}

		var hudManager = HudManager.Instance;
		if (hudManager == null)
		{
			return;
		}

		this.coroutine = hudManager.StartCoroutine(routine().WrapToIl2Cpp());
	}

	private void stopRoutine()
	{
		if (this.coroutine == null)
		{
			return;
		}

		var hudManager = HudManager.Instance;
		if (hudManager != null)
		{
			hudManager.StopCoroutine(this.coroutine);
		}
		this.coroutine = null;
	}

	private void applyShake(float offsetX, float offsetY)
	{
		var camera = Camera.main;
		if (camera == null)
		{
			return;
		}

		Vector3 position = camera.transform.position;
		position.x = position.x - this.appliedShakeX + offsetX;
		position.y = position.y - this.appliedShakeY + offsetY;

		camera.transform.position = position;
		this.appliedShakeX = offsetX;
		this.appliedShakeY = offsetY;
	}

	private void resetShake()
	{
		if (!this.isShaking)
		{
			return;
		}

		applyShake(0.0f, 0.0f);
		this.isShaking = false;
	}
}

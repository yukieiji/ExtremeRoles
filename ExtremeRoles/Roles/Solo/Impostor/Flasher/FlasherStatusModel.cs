using UnityEngine;

using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor.Flasher;

/// <summary>
/// フラッシュの色。翻訳キーは列挙名をそのまま使う。
/// </summary>
public enum FlasherColor
{
	FlasherColorWhite,
	FlasherColorBlack,
}

/// <summary>
/// フラッシャーが保持する状態と、オプション化しない固定値を保持する。
/// </summary>
public sealed class FlasherStatusModel : IStatusModel
{
	/// <summary>チャージが満タンになるまでの時間</summary>
	public const float ChargeTime = 3.0f;

	/// <summary>満タンチャージ時に付与される余韻の時間</summary>
	public const float AfterglowMaxTime = 1.5f;

	/// <summary>フラッシュが当たる距離</summary>
	public float Range { get; }

	/// <summary>フラッシュの色</summary>
	public FlasherColor FlashColor { get; }

	/// <summary>直前の発動で溜まっていたチャージ量(0.0f ~ 1.0f)</summary>
	public float ChargeGage { get; private set; }

	/// <summary>発動から余韻への移行または強制終了まで継続する</summary>
	public bool IsSequenceActive { get; private set; }

	public FlasherStatusModel(float range, FlasherColor flashColor)
	{
		this.Range = range;
		this.FlashColor = flashColor;
	}

	/// <summary>
	/// 演出シーケンスを開始する。
	/// </summary>
	public void StartSequence(float chargeGage)
	{
		this.ChargeGage = clampChargeGage(chargeGage);
		this.IsSequenceActive = true;
	}

	/// <summary>
	/// 演出シーケンスを終了する。
	/// </summary>
	public void EndSequence()
	{
		this.ChargeGage = 0.0f;
		this.IsSequenceActive = false;
	}

	/// <summary>
	/// チャージ量に対応する余韻の時間を返す。
	/// </summary>
	public float CalculateAfterglowTime()
		=> clampChargeGage(this.ChargeGage) * AfterglowMaxTime;

	/// <summary>
	/// RPCでやり取りする色を画面の色に変換する。
	/// </summary>
	public static Color ToColor(FlasherColor flashColor)
		=> flashColor is FlasherColor.FlasherColorBlack ? Color.black : Color.white;

	private static float clampChargeGage(float chargeGage)
		=> Mathf.Clamp01(chargeGage);
}

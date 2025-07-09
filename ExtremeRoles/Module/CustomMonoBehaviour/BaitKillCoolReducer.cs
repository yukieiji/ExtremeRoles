using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BaitKillCoolReducer : MonoBehaviour
{
	public float Timer { private get; set; } = 0.0f;
	public float ReduceMulti { private get; set; } = 1.0f;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private PlayerControl localPlayer;
	public BaitKillCoolReducer(System.IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public void Awake()
	{
		this.localPlayer = PlayerControl.LocalPlayer;
	}

	public void FixedUpdate()
	{
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			this.Timer <= 0.0f)
		{
			return;
		}

		float deltaTime = Time.fixedDeltaTime;
		this.Timer -= deltaTime;

		if (this.localPlayer == null ||
			this.localPlayer.Data == null ||
			this.localPlayer.Data.IsDead ||
			this.localPlayer.Data.Disconnected)
		{
			Destroy(this);
			return;
		}

		this.localPlayer.SetKillTimer(
			this.localPlayer.killTimer - (deltaTime * this.ReduceMulti));
		if (this.Timer <= 0.0f)
		{
			Destroy(this);
		}
	}
}

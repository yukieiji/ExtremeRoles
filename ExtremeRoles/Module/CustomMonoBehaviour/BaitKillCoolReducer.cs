using UnityEngine;

using ExtremeRoles.Extension.Player;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BaitKillCoolReducer : MonoBehaviour
{
	private float timer = 0.0f;
	private float reduceMulti = 1.0f;

	public readonly record struct InitializeParameter(float Timer, float ReduceMulti, ContinueChecker Checker);
	public InitializeParameter Parameter
	{
		set
		{
			this.timer = value.Timer;
			this.reduceMulti = value.ReduceMulti;
			this.checker = value.Checker;
		}
	}

	public sealed class ContinueChecker(bool isCheck, PlayerControl reporter)
	{
		public bool IsCheck { get; private set; } = isCheck;
		
		// レポーターが死んでいると何もしない
		public bool IsReduce => this.reporter.IsAlive();

		private readonly PlayerControl reporter = reporter;

		public void DisableCheck()
		{
			this.IsCheck = false;
		}
	}
	private ContinueChecker? checker;

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
		if (this.checker is null)
		{
			return;
		}

		if (this.checker.IsCheck)
		{
			// 一回会議が入ったのでずっとOK
			if (MeetingHud.Instance != null)
			{
				this.checker.DisableCheck();
				return;
			}
			
			if (!this.checker.IsReduce)
			{
				Destroy(this);
				return;
			}
		}

		if (MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			this.timer <= 0.0f)
		{
			return;
		}

		float deltaTime = Time.fixedDeltaTime;
		this.timer -= deltaTime;

		if (this.localPlayer.IsDead())
		{
			Destroy(this);
			return;
		}

		this.localPlayer.SetKillTimer(
			this.localPlayer.killTimer - (deltaTime * this.reduceMulti));
		if (this.timer <= 0.0f)
		{
			Destroy(this);
		}
	}
}

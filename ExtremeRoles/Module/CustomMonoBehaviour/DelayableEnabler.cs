using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

[Il2CppRegister]
public sealed class DelayableEnabler : MonoBehaviour
{
	private float delayTimer = 0.0f;
	private MonoBehaviour? enableMono;

	private DelayableEnabler enabler;
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public DelayableEnabler(System.IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	public void Awake()
	{
		this.enabler = this;
	}

	public void Initialize(MonoBehaviour mono, float delaySecond)
	{
		this.enableMono = mono;
		this.delayTimer = delaySecond;

		this.enableMono.enabled = false;
	}

	public void FixedUpdate()
	{
		if (this.enableMono == null) { return; }

		this.delayTimer -= Time.fixedDeltaTime;

		if (this.delayTimer < 0.0f)
		{
			this.enableMono.enabled = true;
			this.enableMono = null;
			Object.Destroy(this.enabler);
		}
	}
}
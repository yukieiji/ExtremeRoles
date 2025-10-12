using System;

using UnityEngine;

using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

#nullable enable

[Il2CppRegister]
public sealed class IronMateDeadBody : MonoBehaviour
{
	public IronMateDeadBody(IntPtr ptr) : base(ptr) { }

	private DeadBody? deadBody;
	private FakerDummySystem.FakePlayer? fake;
	private SpriteRenderer[]? rends;
	private float maxTime;
	private float timer;

	private float deadBodyShowTime;
	private float fakeShowTime;

	public void SetUp(PlayerControl target, float deadBodyShowTime, float fakeShowTime)
	{
		this.deadBody = base.gameObject.GetComponent<DeadBody>();
		foreach (var rend in this.deadBody.bodyRenderers)
		{
			rend.enabled = false;
		}

		this.fakeShowTime = fakeShowTime;
		this.deadBodyShowTime = deadBodyShowTime;

		if (this.fakeShowTime > 0.0f)
		{
			this.fake = new FakerDummySystem.FakePlayer(
				PlayerControl.LocalPlayer,
				target, false);

			this.fake.Body.transform.position = base.transform.position;

			this.maxTime = this.fakeShowTime;
			this.rends = this.fake.Body.GetComponentsInChildren<SpriteRenderer>();
		}
		else
		{
			this.changeToRend();
		}
	}

	public void FixedUpdate()
	{
		if (this.rends == null ||
			this.deadBody == null)
		{
			return;
		}

		var deadBodyRend = this.deadBody.bodyRenderers[0];
		if (deadBodyRend.enabled &&
			deadBodyRend.color.a <= 0.0f)
		{
			this.enabled = false;
			return;
		}

		if (this.timer >= this.maxTime)
		{
			this.changeToRend();
			return;
		}

		this.timer += Time.fixedDeltaTime;

		foreach (SpriteRenderer rend in this.rends)
		{
			var color = rend.color;
			rend.color = new Color(color.r, color.g, color.b, 1 - (this.timer / this.maxTime));
		}
	}

	public void OnDestroy()
	{
		if (this.fake == null)
		{
			return;
		}
		this.fake.Clear();
	}

	private void changeToRend()
	{
		if (this.deadBody == null)
		{
			return;
		}

		this.timer = 0;
		foreach (var rend in this.deadBody.bodyRenderers)
		{
			rend.enabled = true;
		}

		this.rends = this.deadBody.bodyRenderers;
		this.maxTime = this.deadBodyShowTime;

		if (this.fake != null)
		{
			this.fake.Clear();
		}

		if (this.deadBodyShowTime > 0.0f)
		{
			return;
		}

		var targetColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		foreach (SpriteRenderer rend in this.rends)
		{
			rend.color = targetColor;
		}

		if (this.gameObject.TryGetComponent<ViperDeadBody>(out var viperDeadBody))
		{
			// dissolveCurrentTimeをTime.fixedDeltaTime引いた時に必ず0秒以下にするためにこうする
			viperDeadBody.dissolveCurrentTime = Time.fixedDeltaTime - 0.00001f;
			// そしてFixedUpdateを呼び出す
			viperDeadBody.FixedUpdate();
		}
	}
}

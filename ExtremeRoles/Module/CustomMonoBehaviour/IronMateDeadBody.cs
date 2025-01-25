using ExtremeRoles.Module.SystemType.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
		this.fake = new FakerDummySystem.FakePlayer(
			PlayerControl.LocalPlayer,
			target, false);

		this.fake.Body.transform.position = base.transform.position;

		this.fakeShowTime = fakeShowTime;
		this.deadBodyShowTime = deadBodyShowTime;

		if (this.fakeShowTime > 0.0f)
		{
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
		if (this.fake == null || this.rends == null || this.deadBody == null)
		{
			return;
		}

		if (this.deadBody.bodyRenderers[0].enabled &&
			this.deadBody.bodyRenderers[0].color.a <= 0)
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
		if (this.deadBody == null || this.fake == null || this.rends == null)
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
		this.fake.Clear();
	}
}

using UnityEngine;

using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class TimerSpeedMod : MonoBehaviour
{
	private float timer;
	private PlayerSpeedSystem? system;

	public TimerSpeedMod(System.IntPtr ptr) : base(ptr) { }

	public void SetUp(float newSpeed, float timer)
	{
		this.timer = timer;
		this.system = PlayerSpeedSystem.Get();
		this.system.UpdateSelfSpeed(newSpeed);
	}

	public void FixedUpdate()
	{
		if (this.timer <= 0.0f)
		{
			this.system?.UpdateSelfSpeed(0.0f);
			Destroy(this);
			return;
		}

		this.timer -= Time.fixedDeltaTime;
	}
}

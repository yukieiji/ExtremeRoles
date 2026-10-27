using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.SystemType;

using Il2CppInterop.Runtime.Attributes;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SpeedMod : MonoBehaviour
{
	public const string SpeedId = "SpeedMod_Speed";

	private float timer;
	private float speedMultiplier = 1.0f;
	private SingleRoleBase? role;
	private bool isSpeedApplied = false;

	public SpeedMod(System.IntPtr ptr) : base(ptr) { }

	public void SetUp(float speed, float timer)
	{
		this.timer = timer;
		this.role = ExtremeRoleManager.GetLocalPlayerRole();
		this.speedMultiplier = speed;
		applySpeed();
	}

	public void FixedUpdate()
	{
		if (this.role is null)
		{
			return;
		}

		if (this.timer <= 0.0f)
		{
			if (this.isSpeedApplied && PlayerControl.LocalPlayer != null)
			{
				PlayerSpeedSystem.Get().RemoveSpeed(PlayerControl.LocalPlayer.PlayerId, SpeedId);
				this.isSpeedApplied = false;
			}
			return;
		}

		this.timer -= Time.fixedDeltaTime;

		applySpeed();
	}

	[HideFromIl2Cpp]
	private void applySpeed()
	{
		if (!this.isSpeedApplied && PlayerControl.LocalPlayer != null)
		{
			PlayerSpeedSystem.Get().SetSpeed(PlayerControl.LocalPlayer.PlayerId, SpeedId, this.speedMultiplier);
			this.isSpeedApplied = true;
		}
	}
}

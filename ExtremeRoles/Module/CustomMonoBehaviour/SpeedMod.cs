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
	private SingleRoleBase? role;

	public SpeedMod(System.IntPtr ptr) : base(ptr) { }

	public void SetUp(float speed, float timer)
	{
		this.timer = timer;
		this.role = ExtremeRoleManager.GetLocalPlayerRole();
		if (PlayerControl.LocalPlayer != null)
		{
			PlayerSpeedSystem.Get().SetSpeed(PlayerControl.LocalPlayer.PlayerId, SpeedId, speed);
		}
	}

	public void FixedUpdate()
	{
		if (this.role is null)
		{
			return;
		}

		if (this.timer <= 0.0f)
		{
			if (PlayerControl.LocalPlayer != null)
			{
				PlayerSpeedSystem.Get().RemoveSpeed(PlayerControl.LocalPlayer.PlayerId, SpeedId);
			}
			Destroy(this);
			return;
		}

		this.timer -= Time.fixedDeltaTime;
	}
}

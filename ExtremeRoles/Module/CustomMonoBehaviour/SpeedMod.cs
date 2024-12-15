using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SpeedMod : MonoBehaviour
{
	private float timer;
	private float speed;

	public void SetUp(float speed, float timer)
	{
		this.speed = speed;
		this.timer = timer;

		updateSpeed();
	}

	public void FixedUpdate()
	{
		if (this.timer <= 0.0f)
		{
			return;
		}

		this.timer -= Time.fixedDeltaTime;

		updateSpeed();
	}
	private void updateSpeed()
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();
		if (role == null)
		{
			return;
		}
		role.MoveSpeed = this.speed;
	}
}

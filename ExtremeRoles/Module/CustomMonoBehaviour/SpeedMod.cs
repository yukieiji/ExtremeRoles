using AmongUs.GameOptions;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class SpeedMod : MonoBehaviour
{
	private float timer;
	private float targetSpeed;
	private float defaultSpeed = float.MinValue;
	private SingleRoleBase? role;

	public SpeedMod(System.IntPtr ptr) : base(ptr) { }

	public void SetUp(float speed, float timer)
	{
		this.timer = timer;
		this.role = ExtremeRoleManager.GetLocalPlayerRole();
		this.defaultSpeed = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
			FloatOptionNames.PlayerSpeedMod);
		this.targetSpeed = this.defaultSpeed * speed;
		updateSpeed(this.role);
	}

	public void FixedUpdate()
	{
		if (this.role is null)
		{
			return;
		}

		if (this.timer <= 0.0f)
		{
			if (this.role.IsBoost)
			{
				this.role.IsBoost = false;
				this.role.MoveSpeed = this.defaultSpeed;
			}
			return;
		}

		this.timer -= Time.fixedDeltaTime;

		updateSpeed(this.role);
	}
	private void updateSpeed(SingleRoleBase role)
	{
		role.IsBoost = true;
		role.MoveSpeed = this.targetSpeed;
	}
}

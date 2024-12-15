using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;

using Hazel;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class TimeBreakerTimeBreakSystem(float activeTime, bool effectImp, bool activeScreen) : IDirtableSystemType
{
	private readonly float activeTime = activeTime;
	private readonly bool effectImp = effectImp;
	private readonly bool activeScreen = activeScreen;

	public bool Active { get; private set; }

	private float timer = 0.0f;
	public bool IsDirty => false;

	private SpriteRenderer? screen;

	public void Deteriorate(float deltaTime)
	{
		if (this.timer <= 0.0f)
		{
			return;
		}
		this.timer -= deltaTime;

		if (this.timer <= 0.0f)
		{
			this.Active = false;
			this.timer = 0.0f;
			if (this.screen != null)
			{
				this.screen.enabled = false;
			}
		}
	}

	public void Deserialize(MessageReader reader, bool initialState)
	{ }

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{
		if (timing == ResetTiming.MeetingStart)
		{
			this.Active = false;
			this.timer = 0.0f;
			if (this.screen != null)
			{
				this.screen.enabled = false;
			}
		}
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		var role = ExtremeRoleManager.GetLocalPlayerRole();

		if (!this.effectImp && (role.IsImpostor() || role.Id == ExtremeRoleId.Marlin))
		{
			return;
		}

		this.Active = true;
		this.timer += this.activeTime;

		if (!this.activeScreen)
		{
			return;
		}

		if (screen == null)
		{
			screen = Object.Instantiate(
				 FastDestroyableSingleton<HudManager>.Instance.FullScreen,
				 FastDestroyableSingleton<HudManager>.Instance.transform);
			screen.transform.localPosition = new Vector3(0f, 0f, 20f);
			screen.gameObject.SetActive(true);
			screen.enabled = false;
			screen.color = new Color(0f, 0.5f, 0.8f, 0.3f);
		}
		// Screen On
		screen.enabled = true;
	}
}

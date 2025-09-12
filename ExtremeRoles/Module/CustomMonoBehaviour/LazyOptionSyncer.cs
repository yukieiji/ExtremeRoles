using System;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class LazyOptionSyncer : MonoBehaviour
{
	private float timer = 0.0f;
	public bool Wait { get; private set; } = false;
	private const float maxTimer = 1.0f;

	public LazyOptionSyncer(IntPtr ptr) : base(ptr)
	{
	}

	public void Update()
	{
		if (!this.Wait)
		{
			return;
		}

		if (this.timer > 0.0f)
		{
			this.timer -= Time.deltaTime;
			return;
		}
		syncOption();
	}

	public void SyncOption()
	{
		if (this.timer > 0.0f)
		{
			this.timer = maxTimer;
			return;
		}

		syncOption();
		
		this.timer = maxTimer;
		this.Wait = true;
	}

	private void syncOption()
	{
		var mng = GameManager.Instance;

		if (!AmongUsClient.Instance.AmHost ||
			mng == null ||
			mng.LogicOptions == null)
		{
			return;
		}

		this.Wait = false;

		var opti = mng.LogicOptions;
		opti.SetDirty();

		var pc = PlayerControl.LocalPlayer;
		if (pc != null)
		{
			pc.RpcSyncSettings(
				opti.gameOptionsFactory.ToBytes(
					opti.currentGameOptions,
					AprilFoolsMode.IsAprilFoolsModeToggledOn));
		}
	}
}

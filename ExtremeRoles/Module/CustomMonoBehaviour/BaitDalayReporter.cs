using System;
using System.Collections;
using TMPro;
using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils;

using Il2CppInterop.Runtime.Attributes;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BaitDalayReporter : MonoBehaviour
{

	private TextMeshPro? text = null;
	private Coroutine? delayCorutine = null;

	private readonly FullScreenFlasher flasher = new FullScreenFlasher(ColorPalette.BaitCyan, 0.75f, 0.5f, 0.5f);

	public BaitDalayReporter(IntPtr ptr) : base(ptr)
	{
	}

	public void FixedUpdate()
	{
		if (MeetingHud.Instance == null)
		{
			return;
		}

		this.stopTargetCorutine(this.delayCorutine);

		this.flasher.Reset();

		if (this.text != null)
		{
			Destroy(this.text);
		}

		Destroy(this);
	}

	public void StartReportTimer(
		NetworkedPlayerInfo target,
		float timer = 0.0f)
	{
		flasher.Flash();

		var player = PlayerControl.LocalPlayer;
		if (timer == 0)
		{
			reportTarget(target);
		}
		else
		{
			this.delayCorutine = this.StartCoroutine(
				this.delayReport(timer, target));
		}
	}

	[HideFromIl2Cpp]
	private IEnumerator delayReport(float targetTime, NetworkedPlayerInfo target)
	{
		if (this.text == null)
		{
			this.text = Instantiate(
				HudManager.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.text.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.text.enableWordWrapping = false;
		}

		string placeholder = Tr.GetString("forceReportUntil");

		do
		{
			targetTime -= Time.deltaTime;
			this.text.gameObject.SetActive(true);
			this.text.text = string.Format(
				placeholder, Mathf.CeilToInt(targetTime));
			yield return null;
		}
		while (targetTime >= 0.0f);

		reportTarget(target);
	}

	private static void reportTarget(NetworkedPlayerInfo target)
	{
		PlayerControl.LocalPlayer.CmdReportDeadBody(target);
	}

	private void stopTargetCorutine(Coroutine? coroutine)
	{
		if (coroutine != null)
		{
			this.StopCoroutine(coroutine);
		}
	}
}

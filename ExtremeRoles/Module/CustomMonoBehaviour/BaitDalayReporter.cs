using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using System.Collections;
using TMPro;
using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils;

using FloatAction = System.Action<float>;
using Il2CppInterop.Runtime.Attributes;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BaitDalayReporter : MonoBehaviour
{
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private SpriteRenderer rend;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	private TextMeshPro? text = null;

	private Coroutine? flushCorutine = null;
	private Coroutine? delayCorutine = null;

	public void Awake()
	{
		var hudManager = FastDestroyableSingleton<HudManager>.Instance;

		this.rend = Instantiate(
			hudManager.FullScreen, hudManager.transform);
		this.rend.transform.localPosition = new Vector3(0f, 0f, 20f);
		this.rend.gameObject.SetActive(true);
	}

	public void FixedUpdate()
	{
		if (MeetingHud.Instance == null) { return; }

		this.stopTargetCorutine(this.flushCorutine);
		this.stopTargetCorutine(this.delayCorutine);

		Destroy(this.rend);

		if (this.text != null)
		{
			Destroy(this.text);
		}

		Destroy(this);
	}

	public void StartReportTimer(
		Color color,
		NetworkedPlayerInfo target,
		float timer = 0.0f)
	{
		this.rend.enabled = true;

		this.flushCorutine = this.StartCoroutine(
			Effects.Lerp(1.0f, new FloatAction((p) =>
			{
				if (this.rend == null) { return; }

				float progress = p < 0.5 ?　p :　(1 - p);
				float alpha = Mathf.Clamp01(progress * 2 * 0.75f);

				this.rend.color = new Color(
					color.r, color.g,
					color.b, alpha);

				if (p == 1f)
				{
					this.rend.enabled = false;
				}
			}))
		);

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
				FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
				Camera.main.transform, false);
			this.text.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
			this.text.enableWordWrapping = false;
		}

		string placeholder = OldTranslation.GetString("forceReportUntil");

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

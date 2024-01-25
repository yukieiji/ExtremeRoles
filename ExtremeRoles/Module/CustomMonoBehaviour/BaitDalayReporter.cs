using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using TMPro;
using UnityEngine;

using FloatAction = System.Action<float>;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class BaitDalayReporter : MonoBehaviour
{
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private SpriteRenderer rend;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	private TextMeshPro? text;

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

		this.StopAllCoroutines();

		Destroy(this.rend);

		if (this.text != null)
		{
			Destroy(this.text);
		}

		Destroy(this);
	}

	public void StartReportTimer(
		Color color,
		GameData.PlayerInfo target,
		float timer = 0.0f)
	{
		this.rend.enabled = true;

		this.StartCoroutine(
			Effects.Lerp(1.0f, new FloatAction((p) =>
			{
				if (this.rend == null) { return; }

				float alpha = p < 0.5 ?
					Mathf.Clamp01(p * 2 * 0.75f) :
					Mathf.Clamp01((1 - p) * 2 * 0.75f);

				this.rend.color = new Color(
					color.r, color.g,
					color.b, alpha);

				if (p == 1f)
				{
					this.rend.enabled = false;
				}
			}))
		);

		var player = CachedPlayerControl.LocalPlayer.PlayerControl;
		if (timer == 0)
		{
			reportTarget(target);
		}
		else
		{
			this.StartCoroutine(
				Effects.Lerp(timer, new FloatAction((time) =>
				{
					if (this.text == null)
					{
						this.text = Instantiate(
							FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
							Camera.main.transform, false);
						this.text.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
						this.text.enableWordWrapping = false;
					}

					this.text.gameObject.SetActive(true);
					this.text.text = string.Format(
						Translation.GetString("forceReportUntil"),
						Mathf.CeilToInt(timer - time));

					if (time == timer)
					{
						reportTarget(target);
					}
				}))
			);
		}
	}
	private static void reportTarget(GameData.PlayerInfo target)
	{
		CachedPlayerControl.LocalPlayer.PlayerControl.CmdReportDeadBody(target);
	}
}

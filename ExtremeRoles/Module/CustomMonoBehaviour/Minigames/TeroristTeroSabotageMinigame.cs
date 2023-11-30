using System;
using TMPro;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using BepInEx.Unity.IL2CPP.Utils;

using CollectionEnum = System.Collections.IEnumerator;
using SaboTask = ExtremeRoles.Module.SystemType.Roles.TeroristTeroSabotageSystem.TeroSabotageTask;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class TeroristTeroSabotageMinigame : Minigame
{
	public byte TargetBombId { private get; set; } = byte.MaxValue;

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private TextMeshPro progressText;
	private TextMeshPro logText;

	private SimpleButton startButton;
	private SpriteRenderer progress;

	private float maxTimer = 5.0f;

	private const float ProgressBarLastX = 0.8f;
	private const float ProgressBarLastScaleX = 6.0f;
	private const float ProgressBarScaleY = 0.75f;
	private const float ProgressBarX = 2.2f;
	private const float ProgressBarY = 2.75f;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	private SaboTask? task;

	public override void Begin(PlayerTask task)
	{
		this.AbstractBegin(task);

		if (!task.IsTryCast<ExtremePlayerTask>(out var playerTask) ||
			playerTask!.Behavior is not SaboTask saboTask)
		{
			throw new ArgumentException("invalided Task");
		}
		this.task = saboTask;

		var trans = base.transform;

		this.progressText = trans.Find("ProgressText").GetComponent<TextMeshPro>();
		this.logText = trans.Find("LogText").GetComponent<TextMeshPro>();

		this.progress = trans.Find("Progress").GetComponent<SpriteRenderer>();
		this.progress.gameObject.SetActive(false);

		this.startButton = trans.Find("SimpleButton").GetComponent<SimpleButton>();
		this.startButton.Awake();

		this.startButton.gameObject.SetActive(true);
		this.startButton.ClickedEvent.AddListener(
			() =>
			{
				this.StartCoroutine(coStartActive());
			});
	}

	private CollectionEnum coStartActive()
	{
		if (this.task is null)
		{
			throw new ArgumentException("invalided Task");
		}

		this.startButton.gameObject.SetActive(false);
		this.progress.gameObject.SetActive(true);

		float timer = 0.0f;

		this.updateProgress(0.0f);

		while (timer < maxTimer)
		{
			yield return new WaitForFixedUpdate();
			timer += Time.fixedDeltaTime;
			this.updateProgress(timer);
		}
		this.updateProgress(maxTimer);
		this.task.Next(this.TargetBombId);

		yield return new WaitForSeconds(0.5f);

		this.Close();
	}

	private void updateProgress(float timer)
	{
		float progress = timer / maxTimer;
		float pow_progerss = Mathf.Pow(progress, 1 / (float)2.0f);

		this.progress.transform.localPosition = new Vector3(-ProgressBarX + ((ProgressBarX + ProgressBarLastX) * pow_progerss), ProgressBarY);
		this.progress.transform.localScale = new Vector3(pow_progerss * ProgressBarLastScaleX, ProgressBarScaleY);
	}
}

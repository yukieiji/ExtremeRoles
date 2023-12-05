using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using BepInEx.Unity.IL2CPP.Utils;
using Il2CppInterop.Runtime.Attributes;

using CollectionEnum = System.Collections.IEnumerator;
using SaboTask = ExtremeRoles.Module.SystemType.Roles.TeroristTeroSabotageSystem.Task;
using ConsoleInfo = ExtremeRoles.Module.SystemType.Roles.TeroristTeroSabotageSystem.ConsoleInfo;
using System.Text;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

[Il2CppRegister]
public sealed class TeroristTeroSabotageMinigame : Minigame
{
	public byte BombId => this.ConsoleInfo.BombId;
	public ConsoleInfo ConsoleInfo { private get; set; }

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	private TextMeshPro progressText;
	private TextMeshPro logText;

	private SimpleButton startButton;
	private SpriteRenderer progress;
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	private readonly StringBuilder logBuilder = new StringBuilder();
	private readonly Queue<string> showLogText = new Queue<string>(showMaxTextLine);
	private string[]? logTextArray;
	private int logSelector;

	private const int showMaxTextLine = 19;
	private const float ProgressBarLastX = 0.8f;
	private const float ProgressBarLastScaleX = 4.0f;
	private const float ProgressBarScaleY = 0.75f;
	private const float ProgressBarX = 0.75f;
	private const float ProgressBarY = 2.0f;

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

		this.logText.text = "";
		this.logTextArray = allLog;

		this.startButton.gameObject.SetActive(true);
		this.startButton.ClickedEvent.AddListener(
			() =>
			{
				this.StartCoroutine(coStartActive());
			});
	}

	[HideFromIl2Cpp]
	private CollectionEnum coStartActive()
	{
		if (this.task is null)
		{
			throw new ArgumentException("invalided Task");
		}

		this.startButton.gameObject.SetActive(false);
		this.progress.gameObject.SetActive(true);
		this.logSelector = -1;

		float timer = 0.0f;
		float maxTime = CachedPlayerControl.LocalPlayer.Data.IsDead ?
			this.ConsoleInfo.DeadPlayerActivateTime :
			this.ConsoleInfo.PlayerActivateTime;

		this.updateProgress(0.0f, maxTime);

		while (timer < maxTime)
		{
			yield return new WaitForFixedUpdate();
			timer += Time.fixedDeltaTime;
			this.updateProgress(timer, maxTime);
		}
		this.updateProgress(maxTime, maxTime);
		this.task.Next(this.ConsoleInfo.BombId);

		yield return new WaitForSeconds(0.5f);

		this.Close();
	}

	private void updateProgress(float timer, in float maxTimer)
	{
		float progress = timer / maxTimer;
		float pow_progerss = Mathf.Pow(progress, 1 / (float)2.0f);

		this.progress.transform.localPosition = new Vector3(-ProgressBarX + ((ProgressBarX + ProgressBarLastX) * pow_progerss), ProgressBarY);
		this.progress.transform.localScale = new Vector3(pow_progerss * ProgressBarLastScaleX, ProgressBarScaleY);

		if (this.logTextArray is null) { return; }

		float textProgress = progress * (this.logSelector + 1 / this.logTextArray.Length);
		int newSelecter = Mathf.CeilToInt(textProgress);

		if (newSelecter == this.logSelector) { return; }

		string newLog = this.logTextArray[newSelecter];
		while (this.showLogText.Count > showMaxTextLine)
		{
			this.showLogText.Dequeue();
		}
		this.showLogText.Enqueue(newLog);
		this.logBuilder.Clear();
		foreach (string text in this.showLogText)
		{
			this.logBuilder.AppendLine(text);
		}
		this.logText.text = this.logBuilder.ToString();
	}

	private static string[] allLog =
	[
		"unpacking..... 0％",
		"unpacking..... 10％",
		"unpacking..... 20％",
		"unpacking..... 30％",
		"unpacking..... 40％",
		"unpacking..... 50％",
		"unpacking..... 60％",
		"unpacking..... 70％",
		"unpacking..... 80％",
		"unpacking..... 90％",
		"unpacking..... 100％",
		"---- starting : Bomb cracking system v9.2.0.0 -----",
		"start: firewall breaking",
		"progress..... 0％",
		"progress..... 10％",
		"progress..... 20％",
		"progress..... 30％",
		"progress..... 40％",
		"progress..... 50％",
		"progress..... 60％",
		"progress..... 70％",
		"progress..... 80％",
		"progress..... 90％",
		"progress..... 100％",
		"end: firewall breaking",
		"start: protection overriding",
		"progress..... 0％",
		"progress..... 10％",
		"progress..... 20％",
		"progress..... 30％",
		"progress..... 40％",
		"progress..... 50％",
		"progress..... 60％",
		"progress..... 70％",
		"progress..... 80％",
		"progress..... 90％",
		"progress..... 100％",
		"end: protection overriding",
		"start: installing main system",
		"progress..... 0％",
		"progress..... 5％",
		"progress..... 10％",
		"progress..... 15％",
		"progress..... 20％",
		"progress..... 25％",
		"progress..... 30％",
		"progress..... 35％",
		"progress..... 40％",
		"progress..... 45％",
		"progress..... 50％",
		"progress..... 55％",
		"progress..... 60％",
		"progress..... 65％",
		"progress..... 70％",
		"progress..... 75％",
		"progress..... 80％",
		"progress..... 85％",
		"progress..... 90％",
		"progress..... 95％",
		"progress..... 100％",
		"end: installing main system",
		"start: installing sub system",
		"progress..... 0％",
		"progress..... 10％",
		"progress..... 20％",
		"progress..... 30％",
		"progress..... 40％",
		"progress..... 50％",
		"progress..... 60％",
		"progress..... 70％",
		"progress..... 80％",
		"progress..... 90％",
		"progress..... 100％",
		"end: installing sub system",
		"-------- start: main process --------",
		"progress..... 0％",
		"progress..... 5％",
		"progress..... 10％",
		"progress..... 15％",
		"progress..... 20％",
		"progress..... 25％",
		"progress..... 30％",
		"progress..... 35％",
		"progress..... 40％",
		"progress..... 45％",
		"progress..... 50％",
		"progress..... 55％",
		"progress..... 60％",
		"progress..... 65％",
		"progress..... 70％",
		"progress..... 75％",
		"progress..... 80％",
		"progress..... 85％",
		"progress..... 90％",
		"progress..... 95％",
		"progress..... 100％",
		"-------- end: main process --------",
		"-------- start: backup process --------",
		"progress..... 0％",
		"progress..... 20％",
		"progress..... 40％",
		"progress..... 60％",
		"progress..... 80％",
		"progress..... 100％",
		"-------- end: backup process --------",
		"---- endting : Bomb cracking system v9.2.0.0 -----",
		"deconstructing...."
	];
}

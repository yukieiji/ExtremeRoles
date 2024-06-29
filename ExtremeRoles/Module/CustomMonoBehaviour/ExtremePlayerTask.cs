using System;
using System.Collections.Generic;

using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Performance;

using Il2CppStringBuilder = Il2CppSystem.Text.StringBuilder;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremePlayerTask : PlayerTask
{
	public interface IBehavior
	{
		public int MaxStep { get; }
		public int TaskStep { get; }
		public string TaskText { get; }

		public TaskTypes TaskTypes { get; }

		public void Initialize(PlayerControl owner, Transform transform);
		public void OnRemove();
		public void OnComplete();

		protected static void CloseMinigame<T>() where T : Minigame
		{
			if (Minigame.Instance.IsTryCast<T>(out var targetMinigame) &&
				targetMinigame.amClosing != Minigame.CloseState.Closing)
			{
				targetMinigame.Close();
			}
		}

		protected static Minigame? GetMinigameFromAsset<T>(string name) where T : Minigame
		{
			GameObject minigameObj = Resources.Loader.GetUnityObjectFromResources<GameObject>("Minigame", name);
			GameObject prefab = Instantiate(minigameObj);
			T minigame = prefab.GetComponent<T>();

			return minigame.TryCast<Minigame>();
		}

		protected static string FlashArrowAndTextWithVanillaSabotage(
			ref bool trigger,
			in string text,
			in IEnumerable<ArrowBehaviour?> arrowContainer)
		{
			trigger = !trigger;

			var color = trigger ? Color.red : Color.yellow;

			string coloredString = Design.ColoedString(color, text);
			foreach (var arrow in arrowContainer)
			{
				if (arrow == null) { continue; }
				arrow.image.color = color;
			}
			return coloredString;
		}
	}

	public static ExtremePlayerTask AddTask(IBehavior task, uint id)
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		ExtremePlayerTask t = new GameObject(task.ToString())
			.AddComponent<ExtremePlayerTask>();

		t.Behavior = task;
		t.transform.SetParent(localPlayer.transform, false);
		t.TaskType = t.Behavior.TaskTypes;
		t.Id = id;
		t.Owner = localPlayer;
		t.MinigamePrefab = null;
		t.Initialize();
		localPlayer.myTasks.Add(t);
		return t;
	}

	[HideFromIl2Cpp]
	public IBehavior? Behavior { get; set; }

	public override int TaskStep =>
		this.Behavior is null ? 0 : this.Behavior.TaskStep;

	public override bool IsComplete =>
		this.Behavior is null ? false : this.TaskStep >= this.Behavior.MaxStep;

	public ExtremePlayerTask(IntPtr ptr) : base(ptr)
	{ }

	public override void Initialize()
	{
		this.Behavior?.Initialize(
			this.Owner, base.gameObject.transform);
	}

	public override void OnRemove()
	{
		this.Behavior?.OnRemove();
	}

	public override bool ValidConsole(global::Console console)
		=> false;

	public override void Complete()
	{
		this.Behavior?.OnComplete();
		PlayerControl.LocalPlayer.RemoveTask(this);
	}

	public override void AppendTaskText(Il2CppStringBuilder sb)
	{
		if (this.Behavior is null) { return; }

		sb.Append(this.Behavior.TaskText);

		int maxStep = this.Behavior.MaxStep;
		if (maxStep > 1)
		{
			sb.Append($" ({this.TaskStep}/{maxStep})");
		}
		sb.AppendLine();
	}
}

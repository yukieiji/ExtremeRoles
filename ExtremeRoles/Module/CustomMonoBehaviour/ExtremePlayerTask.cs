using System;

using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

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

		protected static ArrowBehaviour GetArrowTemplate()
		{
			ArrowBehaviour? template = null;

			foreach (var task in CachedShipStatus.Instance.SpecialTasks)
			{
				if (task == null ||
					!task.IsTryCast<SabotageTask>(out var saboTask) ||
					saboTask!.Arrows.Count == 0)
				{
					continue;
				}
				template = saboTask!.Arrows[0];
				break;
			}
			if (template == null)
			{
				throw new ArgumentNullException("Arrow is Null!!");
			}
			return template;
		}

		protected static void CloseMinigame<T>() where T : Minigame
		{
			if (Minigame.Instance != null &&
				Minigame.Instance.IsTryCast<T>(out var targetMinigame) &&
				targetMinigame!.amClosing != Minigame.CloseState.Closing)
			{
				Minigame.Instance.Close();
			}
		}

		protected static Minigame? GetMinigameFromAsset<T>(string name) where T : Minigame
		{
			GameObject minigameObj = Resources.Loader.GetUnityObjectFromResources<GameObject>("Minigame", name);
			GameObject prefab = Instantiate(minigameObj);
			T minigame = prefab.GetComponent<T>();

			return minigame.TryCast<Minigame>();
		}
	}

	public static ExtremePlayerTask AddTask(IBehavior task, uint id)
	{
		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
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
		CachedPlayerControl.LocalPlayer.PlayerControl.RemoveTask(this);
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

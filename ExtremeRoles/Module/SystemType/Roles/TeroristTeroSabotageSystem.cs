using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Extension.Json;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class TeroristTeroSabotageSystem : ISabotageExtremeSystemType
{
	public readonly record struct Option(
		float ExplosionTime, int BombNum,
		MinigameOption MinigameOption);
	public readonly record struct MinigameOption(
		float PlayerActivateTime, bool CanUseDeadPlayer,
		float DeadPlayerActivateTime);
	public readonly record struct ConsoleInfo(
		byte BombId, float PlayerActivateTime,
		bool CanUseDeadPlayer, float DeadPlayerActivateTime);

	public sealed class BombConsoleBehavior : ExtremeConsole.IBehavior
	{
		private static Minigame prefab
		{
			get
			{
				GameObject obj =
					Loader.GetUnityObjectFromResources<GameObject>(
						Path.TeroristTeroMinigameAsset,
						Path.TeroristTeroMinigamePrefab);
				return obj.GetComponent<TeroristTeroSabotageMinigame>();
			}
		}

		public float CoolTime
		{
			get
			{
				PlayerControl player = CachedPlayerControl.LocalPlayer;

				if (player == null || player.Data == null)
				{
					return float.MaxValue;
				}
				return player.Data.IsDead ? 5.0f : 0.0f;
			}
		}

		public bool IsCheckWall => true;

		private readonly ConsoleInfo info;

		public BombConsoleBehavior(in MinigameOption option, byte bombId)
		{
			this.info = new ConsoleInfo(
				bombId,
				option.PlayerActivateTime,
				option.CanUseDeadPlayer,
				option.DeadPlayerActivateTime);
		}

		public bool CanUse(GameData.PlayerInfo pc)
			=> pc.Object.CanMove && FindTeroSaboTask(pc.Object) &&
			(!pc.IsDead || this.info.CanUseDeadPlayer);

		public void Use()
		{
			PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
			PlayerTask? task = FindTeroSaboTask(localPlayer);
			if (task == null) { return; }
			// Idセット処理
			var minigame = MinigameSystem.Create(prefab);

			if (!minigame.IsTryCast<TeroristTeroSabotageMinigame>(out var teroMiniGame))
			{
				throw new ArgumentException("Minigame Missing");
			}
			teroMiniGame!.ConsoleInfo = info;
			teroMiniGame!.Begin(task);
		}
	}

	public sealed class Task : ExtremePlayerTask.IBehavior
	{
		public int MaxStep => this.system.setNum;
		public int TaskStep => this.MaxStep - this.system.setBomb.Count;

		public string TaskText =>
			string.Format(
				Translation.GetString("TeroristBombTask"),
				Mathf.CeilToInt(this.system.ExplosionTimer));

		public TaskTypes TaskTypes => TeroristoTaskTypes;

		private ArrowBehaviour[] arrow;
		private readonly TeroristTeroSabotageSystem system;

		public Task(TeroristTeroSabotageSystem system)
		{
			this.system = system;
			this.arrow = new ArrowBehaviour[this.system.setNum];
		}

		public void Next(byte id)
		{
			HideArrow(id);
			ExtremeSystemTypeManager.RpcUpdateSystem(
				SystemType, x =>
				{
					x.Write((byte)Ops.Cancel);
					x.Write(id);
				});
		}

		public void HideArrow(int index)
		{
			this.arrow[index].gameObject.SetActive(false);
		}

		public void Initialize(PlayerControl owner, Transform transform)
		{
			if (owner == null || !owner.AmOwner) { return; }

			ArrowBehaviour arrow = ExtremePlayerTask.IBehavior.GetArrowTemplate();

			foreach (var (index, console) in this.system.setBomb)
			{
				var targetArrow = UnityObject.Instantiate(arrow, transform);

				targetArrow.target = console.transform.position;
				targetArrow.gameObject.SetActive(true);

				this.arrow[index] = targetArrow;
			}
		}

		public void OnComplete()
		{
			this.OnRemove();
		}

		public void OnRemove()
		{
			foreach (var arrow in this.arrow)
			{
				UnityObject.Destroy(arrow.gameObject);
			}
		}
	}

	public enum Ops
	{
		Setup,
		Cancel
	}

	public const ExtremeSystemType SystemType = ExtremeSystemType.TeroristTeroSabotage;
	public const TaskTypes TeroristoTaskTypes = (TaskTypes)200;

	public bool IsDirty { get; private set; }
	public float ExplosionTimer { get; private set; }
	public bool IsActive { get; private set; }
	public bool IsBlockOtherSabotage => this.isBlockOtherSabotage && this.IsActive;

	private readonly Dictionary<byte, ExtremeConsole> setBomb = new Dictionary<byte, ExtremeConsole>();
	private readonly HashSet<int> setedId = new HashSet<int>();
	private readonly float bombTimer;
	private readonly int setNum;
	private readonly bool isBlockOtherSabotage;
	private readonly MinigameOption minigameOption;

	private readonly ExtremeConsoleSystem consoleSystem;
	private readonly FullScreenFlusherWithAudio flasher;

	private float syncTimer = 0.0f;

	private JObject? json;

	public TeroristTeroSabotageSystem(in Option option, bool isBlockOtherSabotage)
	{
		this.consoleSystem = ExtremeConsoleSystem.Create();
		this.bombTimer = option.ExplosionTime;
		this.setNum = option.BombNum;
		this.minigameOption = option.MinigameOption;
		this.isBlockOtherSabotage = isBlockOtherSabotage;
		this.flasher = new FullScreenFlusherWithAudio(
			Sound.GetAudio(Sound.SoundType.TeroristSabotageAnnounce),
			new Color32(255, 25, 25, 50) , 2.75f);
	}

	public static ExtremePlayerTask? FindTeroSaboTask(PlayerControl pc, bool ignoreComplete=false)
	{
		foreach (var task in pc.myTasks.GetFastEnumerator())
		{
			if (task != null &&
				(ignoreComplete || !task.IsComplete) &&
				task.IsTryCast<ExtremePlayerTask>(out var playerTask) &&
				playerTask!.Behavior!.TaskTypes == TeroristoTaskTypes)
			{
				return playerTask;
			}
		}
		return null;
	}

	public void Deteriorate(float deltaTime)
	{
		if (MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			!this.IsActive)
		{
			this.flasher.SetActive(false);
			return;
		}


		if (!FindTeroSaboTask(CachedPlayerControl.LocalPlayer))
		{
			ExtremePlayerTask.AddTask(new Task(this), 254);
		}
		this.flasher.SetActive(true);
		this.ExplosionTimer -= deltaTime;
		this.syncTimer -= deltaTime;

		if (this.syncTimer < 0f)
		{
			resetSyncTimer();
			this.IsDirty = true;
		}
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void Deserialize(MessageReader reader, bool initialState)
	{
		int newCount = reader.ReadPackedInt32();

		var newBombState = new HashSet<byte>();
		for (int i = 0; i < newCount; ++i)
		{
			byte bombId = reader.ReadByte();
			newBombState.Add(bombId);
			if (!this.setBomb.ContainsKey(bombId))
			{
				setBombToRandomPos(1, bombId);
			}
		}
		this.ExplosionTimer = reader.ReadSingle();

		List<byte> removeIndex = new List<byte>(this.setBomb.Count);
		foreach (byte id in this.setBomb.Keys)
		{
			if (!newBombState.Remove(id))
			{
				removeIndex.Add(id);
			}
		}
		foreach (byte id in removeIndex)
		{
			removeBomb(id);
		}
		checkAllCancel();
	}

	public void Serialize(MessageWriter writer, bool initialState)
	{
		int count = this.setBomb.Count;
		writer.WritePacked(count);
		foreach (byte bombId in this.setBomb.Keys)
		{
			writer.Write(bombId);
		}
		writer.Write(this.ExplosionTimer);
		resetSyncTimer();
		this.IsDirty = initialState;
	}

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		// ホストのみ
		Ops ops = (Ops)msgReader.ReadByte();
		switch (ops)
		{
			case Ops.Cancel:
				byte cancelBombId = msgReader.ReadByte();
				lock (this.setBomb)
				{
					removeBomb(cancelBombId);
				}
				checkAllCancel();
				this.IsDirty = true;
				break;
			case Ops.Setup:
				setBombToRandomPos(this.setNum, 0);

				this.IsActive = true;
				this.ExplosionTimer = this.bombTimer;
				this.IsDirty = true;

				break;
			default:
				return;
		}
	}

	public void Clear()
	{
		this.ExplosionTimer = 1000.0f;
		this.setedId.Clear();
		this.setBomb.Clear();
		this.flasher.SetActive(false);

		var task = FindTeroSaboTask(CachedPlayerControl.LocalPlayer, true);
		if (task != null)
		{
			task.Complete();
		}
		this.IsActive = false;
	}

	private void checkAllCancel()
	{
		// 爆弾0 => サボ終了
		if (this.setBomb.Count == 0)
		{
			this.Clear();
		}
		else
		{
			this.flasher.SetActive(true);
			this.IsActive = true;
		}
	}

	// マップの設置箇所のIDを返す
	private List<VectorId> getSetPosIndex()
	{
		if (this.json == null)
		{
			this.json = JsonParser.GetJObjectFromAssembly(
				"ExtremeRoles.Resources.JsonData.TeroristBombPoint.json");
			if (this.json == null)
			{
				throw new ArgumentException("Json can't find");
			}

		}

		string key = GameSystem.CurMapKey;

		var result = new List<VectorId>(15);

		JArray posInfo = json.Get<JArray>(key);

		for (int i = 0; i < posInfo.Count; ++i)
		{
			JArray posArr = posInfo.Get<JArray>(i);

			result.Add(
				new VectorId(
					i, new Vector2(
						(float)(posArr[0]),
						(float)(posArr[1]))));
		}
		return result;
	}

	private void resetSyncTimer()
	{
		this.syncTimer = 1.0f;
	}

	private void removeBomb(byte id)
	{
		ExtremePlayerTask? task = FindTeroSaboTask(CachedPlayerControl.LocalPlayer);
		if (task != null &&
			task.Behavior is Task teroSabo)
		{
			teroSabo.HideArrow(id);
		}

		if (Minigame.Instance != null &&
			Minigame.Instance.IsTryCast<TeroristTeroSabotageMinigame>(out var teroMinigame) &&
			teroMinigame!.BombId == id)
		{
			Minigame.Instance.ForceClose();
		}

		if (this.setBomb.TryGetValue(id, out ExtremeConsole? value) ||
			value != null)
		{
			this.setBomb.Remove(id);
			if (value.Image != null)
			{
				value.Image.enabled = false;
			}
			UnityObject.DestroyImmediate(value.gameObject);
		}
	}

	private void setBombToRandomPos(int num, byte startId)
	{
		var setPos = getSetPosIndex();
		setPos.RemoveAll(x => this.setedId.Contains(x.Id));

		var randomPos = setPos
			.OrderBy(x => RandomGenerator.Instance.Next())
			.Take(num);

		byte counter = 0;
		foreach (var pos in randomPos)
		{
			this.setedId.Add(pos.Id);

			byte bombId = (byte)(startId + counter);

			var consoleBehavior = new BombConsoleBehavior(this.minigameOption, bombId);
			var newConsole = this.consoleSystem.CreateConsoleObj(
				pos.Pos, "TeroristBomb", consoleBehavior);

			newConsole.Image!.sprite = Loader.CreateSpriteFromResources(
				Path.TeroristTeroSabotageBomb);

			var colider = newConsole.gameObject.AddComponent<CircleCollider2D>();
			colider.isTrigger = true;
			colider.radius = 0.25f;

			this.setBomb.Add(bombId, newConsole);
			++counter;
		}
	}
}

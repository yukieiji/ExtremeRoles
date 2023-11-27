using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ExtremeRoles.Helper;
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

public sealed class TeroristTeroSabotageSystem : IDeterioratableExtremeSystemType
{
	public sealed class BombConsoleBehavior : ExtremeConsole.IBehavior
	{
		private static Minigame prefab
		{
			get
			{
				GameObject obj =
					Resources.Loader.GetUnityObjectFromPath<GameObject>(
						"F:\\Documents\\UnityProject\\UnityAsset\\ExtremeRoles\\TeroristMinigame.asset",
						"assets\\role\\teroristminigame.prefab");
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
				return player.Data.IsDead ? this.deadPlayerCoolTime : 0.0f;
			}
		}

		public bool IsCheckWall => true;

		private readonly float deadPlayerCoolTime;
		private readonly byte bombId;

		public BombConsoleBehavior(float deadPlayerCoolTime, byte bombId)
		{
			this.deadPlayerCoolTime = deadPlayerCoolTime;
			this.bombId = bombId;
		}

		public bool CanUse(GameData.PlayerInfo pc)
			=> pc.Object.CanMove && FindTeroSaboTask(pc.Object);

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
			teroMiniGame!.TargetBombId = this.bombId;
			teroMiniGame!.Begin(task);
		}
	}

	public sealed class TeroSabotageTask : ExtremePlayerTask.IBehavior
	{
		public int MaxStep => this.system.setNum;
		public int TaskStep => this.system.setBomb.Count - this.MaxStep;

		public string TaskText => "爆弾を解除する";

		public TaskTypes TaskTypes => (TaskTypes)200;

		private ArrowBehaviour[] arrow;
		private readonly TeroristTeroSabotageSystem system;

		public TeroSabotageTask(TeroristTeroSabotageSystem system)
		{
			this.system = system;
			this.arrow = new ArrowBehaviour[this.system.setNum];
		}

		public void Next(byte id)
		{
			int index = (int)id;
			this.arrow[index].gameObject.SetActive(false);
			ExtremeSystemTypeManager.RpcUpdateSystem(
				SystemType, x =>
				{
					x.Write((byte)Ops.Cancel);
					x.Write(id);
				});
		}

		public void Initialize(PlayerControl owner)
		{
			if (owner == null || owner.AmOwner) { return; }

			ArrowBehaviour arrow = ExtremePlayerTask.IBehavior.GetArrowTemplate();

			foreach (var(index, console) in this.system.setBomb)
			{
				var targetArrow = UnityObject.Instantiate(arrow);

				targetArrow.target = console.transform.position;
				targetArrow.gameObject.SetActive(true);

				this.arrow[index] = targetArrow;
			}
		}

		public void OnComplete()
		{ }

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

	public bool IsDirty { get; private set; }
	public float ExplosionTimer { get; private set; }

	private readonly Dictionary<byte, ExtremeConsole> setBomb = new Dictionary<byte, ExtremeConsole>();
	private readonly HashSet<int> setedId = new HashSet<int>();
	private readonly float bombTimer;
	private readonly int setNum;
	private readonly float deadPlayerUseCoolTime;

	private readonly ExtremeConsoleSystem consoleSystem;

	private bool isActive = false;
	private float syncTimer = 0.0f;

	private JObject? json;

	public TeroristTeroSabotageSystem(float bombTimer, int setNum, float deadPlayerUseCoolTime)
	{
		this.consoleSystem = ExtremeConsoleSystem.Create();
		this.bombTimer = bombTimer;
		this.setNum = setNum;
		this.deadPlayerUseCoolTime = deadPlayerUseCoolTime;
	}

	public static ExtremePlayerTask? FindTeroSaboTask(PlayerControl pc)
	{
		foreach (var task in pc.myTasks.GetFastEnumerator())
		{
			if (task.IsTryCast<ExtremePlayerTask>(out var playerTask) &&
				playerTask!.Behavior is TeroSabotageTask &&
				!playerTask.Behavior.IsComplete)
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
			!this.isActive)
		{
			this.flashActiveTo(false);
			return;
		}


		if (!FindTeroSaboTask(CachedPlayerControl.LocalPlayer))
		{
			ExtremePlayerTask.AddTask(new TeroSabotageTask(this), 254);
		}
		this.flashActiveTo(true);
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

				this.isActive = true;
				this.ExplosionTimer = this.bombTimer;
				this.IsDirty = true;

				break;
			default:
				return;
		}
	}

	private void checkAllCancel()
	{
		// 爆弾0 => サボ終了
		if (this.setBomb.Count == 0)
		{
			this.isActive = false;
			this.ExplosionTimer = 1000.0f;
			this.setedId.Clear();
			this.flashActiveTo(false);

			var task = FindTeroSaboTask(CachedPlayerControl.LocalPlayer);
			if (task != null)
			{
				task.Complete();
			}
		}
		else
		{
			this.flashActiveTo(true);
			this.isActive = true;
		}
	}

	// マップの設置箇所のIDを返す
	private List<VectorId> getSetPosIndex()
	{
		if (this.json == null)
		{
			this.json = JsonParser.GetJObjectFromAssembly(
				"ExtremeRoles.Resources.JsonData.ThiefTimePartPoint.json");
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

	// TODO: リアクターフラッシュ
	private void flashActiveTo(bool isActive)
	{

	}

	private void resetSyncTimer()
	{
		this.syncTimer = 1.0f;
	}

	private void removeBomb(byte id)
	{
		if (this.setBomb.TryGetValue(id, out ExtremeConsole? value) ||
			value != null)
		{
			this.setBomb.Remove(id);
			UnityObject.Destroy(value.gameObject);
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

			var consoleBehavior = new BombConsoleBehavior(
				this.deadPlayerUseCoolTime, (byte)(startId + counter));
			var newConsole = this.consoleSystem.CreateConsoleObj(pos.Pos, "TeroristBomb", consoleBehavior);
			// TODO: ここで画像入れる処理

			var colider = newConsole.gameObject.AddComponent<CircleCollider2D>();
			colider.isTrigger = true;
			colider.radius = 0.1f;

			this.setBomb.Add(bombId, newConsole);
		}
	}
}

using System.Collections.Generic;

using Hazel;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public enum EncloserOption
{
	StakeCount,
	MetsuLimit,
	AbilityCoolTime,
	MetsuKillMoney
}

public enum EncloserMode : byte
{
	Stake,
	Metsu
}

public sealed class EncloserStatusModel : IStatusModel
{
	public List<Vector2> Stakes { get; } = new List<Vector2>();
	public bool IsPolygonCompleted { get; set; }
	public int RemainingMetsuCount { get; set; }

	public List<GameObject> StakeGameObjects { get; } = new List<GameObject>();
	public GameObject? LinesGameObject { get; set; }
	public LineRenderer? LineRenderer { get; set; }
	public GameObject? PolygonMeshGameObject { get; set; }
	public MeshFilter? MeshFilter { get; set; }
	public MeshRenderer? MeshRenderer { get; set; }

	public void ClearVisuals()
	{
		foreach (var stakeObj in this.StakeGameObjects)
		{
			if (stakeObj != null)
			{
				Object.Destroy(stakeObj);
			}
		}

		this.StakeGameObjects.Clear();

		if (this.LinesGameObject != null)
		{
			Object.Destroy(this.LinesGameObject);
			this.LinesGameObject = null;
			this.LineRenderer = null;
		}

		if (this.PolygonMeshGameObject != null)
		{
			Object.Destroy(this.PolygonMeshGameObject);
			this.PolygonMeshGameObject = null;
			this.MeshFilter = null;
			this.MeshRenderer = null;
		}

		this.Stakes.Clear();
		this.IsPolygonCompleted = false;
	}
}

public sealed class EncloserAbilityHandler : IAbility, IRoleAutoBuildAbility, IRoleResetMeeting
{
	public ExtremeAbilityButton Button { get; set; } = null!;

	public EncloserMode CurrentMode => this.modeSwitcher?.Current ?? EncloserMode.Stake;

	private readonly EncloserStatusModel status;
	private readonly int stakeCount;
	private readonly int metsuLimit;
	private readonly float abilityCoolTime;
	private readonly int metsuKillMoney;

	private GraphicSwitcher<EncloserMode>? modeSwitcher;

	public EncloserAbilityHandler(
		EncloserStatusModel status,
		int stakeCount,
		int metsuLimit,
		float abilityCoolTime,
		int metsuKillMoney)
	{
		this.status = status;
		this.stakeCount = stakeCount;
		this.metsuLimit = metsuLimit;
		this.abilityCoolTime = abilityCoolTime;
		this.metsuKillMoney = metsuKillMoney;
		this.status.RemainingMetsuCount = metsuLimit;
	}

	public void CreateAbility()
	{
		Sprite bombSprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);

		var stakeGraphic = new ButtonGraphic(Tr.GetString("Stake"), bombSprite);
		var metsuGraphic = new ButtonGraphic(Tr.GetString("Metsu"), bombSprite);

		this.CreateAbilityCountButton(
			stakeGraphic.Text,
			stakeGraphic.Img);

		this.Button.SetLabelToCrewmate();
		this.Button.Behavior.SetCoolTime(this.abilityCoolTime);

		this.modeSwitcher = new GraphicSwitcher<EncloserMode>(
			this.Button.Behavior,
			new GraphicMode<EncloserMode>(EncloserMode.Stake, stakeGraphic),
			new GraphicMode<EncloserMode>(EncloserMode.Metsu, metsuGraphic));

		this.modeSwitcher.Switch(EncloserMode.Stake);
	}

	public bool UseAbility()
	{
		if (this.CurrentMode == EncloserMode.Stake)
		{
			Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();
			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
			{
				caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
				caller.WriteByte((byte)EncloserRpcOpsType.PlaceStake);
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}

			HandlePlaceStake(PlayerControl.LocalPlayer.PlayerId, pos);
			return true;
		}
		else if (this.CurrentMode == EncloserMode.Metsu)
		{
			byte encloserPlayerId = PlayerControl.LocalPlayer.PlayerId;
			List<byte> targets = new List<byte>();
			int nonLiberalKills = 0;

			bool isMonikaImmune = false;
			if (MonikaTrashSystem.TryGet(out var trashSystem) && trashSystem.InvalidPlayer(encloserPlayerId))
			{
				isMonikaImmune = true;
			}

			foreach (var pc in PlayerCache.AllPlayerControl)
			{
				if (pc == null || pc.Data == null || pc.Data.IsDead || pc.Data.Disconnected)
				{
					continue;
				}

				Vector2 pPos = pc.GetTruePosition();
				if (!Encloser.IsPointInPolygon(pPos, this.status.Stakes))
				{
					continue;
				}

				if (ExtremeRoleManager.TryGetRole(pc.PlayerId, out var targetRole))
				{
					if (targetRole.Core.Id is ExtremeRoleId.Leader or ExtremeRoleId.Assassin)
					{
						continue;
					}

					if (targetRole.Core.Id is ExtremeRoleId.Monika && isMonikaImmune)
					{
						continue;
					}

					if (targetRole.AbilityClass is IInvincible invincible && invincible.IsBlockKillFrom(encloserPlayerId))
					{
						continue;
					}

					if (!targetRole.IsLiberal())
					{
						nonLiberalKills++;
					}
				}

				targets.Add(pc.PlayerId);
			}

			float moneyGained = nonLiberalKills * this.metsuKillMoney;

			using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
			{
				caller.WriteByte(encloserPlayerId);
				caller.WriteByte((byte)EncloserRpcOpsType.UseMetsu);
				caller.WriteFloat(moneyGained);
				caller.WriteInt(targets.Count);
				foreach (byte targetId in targets)
				{
					caller.WriteByte(targetId);
				}
			}

			HandleUseMetsu(encloserPlayerId, targets, moneyGained);
			return true;
		}

		return false;
	}

	public bool IsAbilityUse()
	{
		if (this.status.RemainingMetsuCount <= 0)
		{
			return false;
		}

		if (this.CurrentMode == EncloserMode.Stake)
		{
			return IRoleAutoBuildAbility.IsCommonUse() &&
				this.status.Stakes.Count < this.stakeCount;
		}
		else if (this.CurrentMode == EncloserMode.Metsu)
		{
			return IRoleAutoBuildAbility.IsCommonUse() &&
				this.status.IsPolygonCompleted;
		}

		return false;
	}

	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.status.Stakes.Add(pos);

		var stakeObj = new GameObject($"Stake_{this.status.Stakes.Count}");
		var sr = stakeObj.AddComponent<SpriteRenderer>();
		sr.sprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);
		sr.color = ColorPalette.LiberalColor;
		stakeObj.transform.position = new Vector3(pos.x, pos.y, -0.1f);
		this.status.StakeGameObjects.Add(stakeObj);

		if (this.status.Stakes.Count >= this.stakeCount)
		{
			this.status.IsPolygonCompleted = true;
			if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId)
			{
				this.modeSwitcher?.Switch(EncloserMode.Metsu);
			}
		}

		rebuildLinesAndPolygonMesh();
		updateVisualVisibility(encloserPlayerId);
	}

	public void HandleUseMetsu(byte encloserPlayerId, IReadOnlyList<byte> targetPlayerIds, float moneyGained)
	{
		foreach (byte targetId in targetPlayerIds)
		{
			Player.RpcUncheckMurderPlayer(encloserPlayerId, targetId, byte.MinValue);
		}

		if (moneyGained > 0 && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
		{
			LiberalMoneyBankSystem.RpcUpdateSystem(
				encloserPlayerId,
				LiberalMoneyHistory.Reason.AddOnKill,
				moneyGained,
				0.0f);
		}

		this.status.RemainingMetsuCount--;
		this.status.ClearVisuals();

		if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId)
		{
			if (this.status.RemainingMetsuCount > 0)
			{
				this.modeSwitcher?.Switch(EncloserMode.Stake);
			}

			this.Button.Behavior.SetCoolTime(this.abilityCoolTime);
		}
	}

	public void ResetOnMeetingStart()
	{
		Reset();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		Reset();
	}

	public void Reset()
	{
		this.status.ClearVisuals();
		if (PlayerControl.LocalPlayer != null && this.Button != null)
		{
			this.modeSwitcher?.Switch(EncloserMode.Stake);
		}
	}

	public void UpdateVisuals(byte encloserPlayerId)
	{
		updateVisualVisibility(encloserPlayerId);

		if (this.status.MeshRenderer != null && this.status.MeshRenderer.material != null)
		{
			float alpha = Mathf.PingPong(Time.time * 2.0f, 0.3f) + 0.15f;
			Color color = new Color(
				ColorPalette.LiberalColor.r,
				ColorPalette.LiberalColor.g,
				ColorPalette.LiberalColor.b,
				alpha);
			this.status.MeshRenderer.material.color = color;
		}
	}

	private void updateVisualVisibility(byte encloserPlayerId)
	{
		bool isLocalPlayerEncloser = PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId;
		bool showLinesAndPoly = this.status.IsPolygonCompleted || isLocalPlayerEncloser;

		if (this.status.LinesGameObject != null)
		{
			this.status.LinesGameObject.SetActive(showLinesAndPoly);
		}

		if (this.status.PolygonMeshGameObject != null)
		{
			this.status.PolygonMeshGameObject.SetActive(showLinesAndPoly);
		}
	}

	private void rebuildLinesAndPolygonMesh()
	{
		if (this.status.Stakes.Count < 2)
		{
			return;
		}

		if (this.status.LinesGameObject == null)
		{
			this.status.LinesGameObject = new GameObject("EncloserLines");
			this.status.LineRenderer = this.status.LinesGameObject.AddComponent<LineRenderer>();
			this.status.LineRenderer.startWidth = 0.15f;
			this.status.LineRenderer.endWidth = 0.15f;
			this.status.LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			this.status.LineRenderer.startColor = ColorPalette.LiberalColor;
			this.status.LineRenderer.endColor = ColorPalette.LiberalColor;
		}

		int posCount = this.status.Stakes.Count + (this.status.IsPolygonCompleted ? 1 : 0);
		this.status.LineRenderer!.positionCount = posCount;

		for (int i = 0; i < this.status.Stakes.Count; i++)
		{
			this.status.LineRenderer.SetPosition(i, new Vector3(this.status.Stakes[i].x, this.status.Stakes[i].y, -0.05f));
		}

		if (this.status.IsPolygonCompleted)
		{
			this.status.LineRenderer.SetPosition(this.status.Stakes.Count, new Vector3(this.status.Stakes[0].x, this.status.Stakes[0].y, -0.05f));
			buildPolygonMesh();
		}
	}

	private void buildPolygonMesh()
	{
		if (this.status.Stakes.Count < 3)
		{
			return;
		}

		if (this.status.PolygonMeshGameObject == null)
		{
			this.status.PolygonMeshGameObject = new GameObject("EncloserPolygonMesh");
			this.status.MeshFilter = this.status.PolygonMeshGameObject.AddComponent<MeshFilter>();
			this.status.MeshRenderer = this.status.PolygonMeshGameObject.AddComponent<MeshRenderer>();
			this.status.MeshRenderer.material = new Material(Shader.Find("Sprites/Default"));
		}

		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[this.status.Stakes.Count];
		for (int i = 0; i < this.status.Stakes.Count; i++)
		{
			vertices[i] = new Vector3(this.status.Stakes[i].x, this.status.Stakes[i].y, 0.05f);
		}

		int triangleCount = (this.status.Stakes.Count - 2) * 3;
		int[] triangles = new int[triangleCount];
		int tIndex = 0;
		for (int i = 1; i < this.status.Stakes.Count - 1; i++)
		{
			triangles[tIndex++] = 0;
			triangles[tIndex++] = i;
			triangles[tIndex++] = i + 1;
		}

		Color yellowFill = new Color(
			ColorPalette.LiberalColor.r,
			ColorPalette.LiberalColor.g,
			ColorPalette.LiberalColor.b,
			0.3f);

		Color[] colors = new Color[vertices.Length];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = yellowFill;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.RecalculateBounds();

		this.status.MeshFilter!.mesh = mesh;
	}
}

public enum EncloserRpcOpsType : byte
{
	PlaceStake,
	UseMetsu
}

public sealed class Encloser : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{
	public override IStatusModel? Status => this.statusModel;

	public ExtremeAbilityButton Button
	{
		get => this.abilityHandler != null ? this.abilityHandler.Button : null!;
		set
		{
			if (this.abilityHandler != null)
			{
				this.abilityHandler.Button = value;
			}
		}
	}

	private readonly EncloserStatusModel statusModel;
	private EncloserAbilityHandler? abilityHandler;

	private int stakeCount;
	private int metsuLimit;
	private float abilityCoolTime;
	private int metsuKillMoney;

	public Encloser() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Encloser))
	{
		this.statusModel = new EncloserStatusModel();
	}

	public override string GetRoleTag()
	{
		return "Ec";
	}

	public void CreateAbility()
	{
		this.abilityHandler = new EncloserAbilityHandler(
			this.statusModel,
			this.stakeCount,
			this.metsuLimit,
			this.abilityCoolTime,
			this.metsuKillMoney);

		this.AbilityClass = this.abilityHandler;
		this.abilityHandler.CreateAbility();
	}

	public bool UseAbility()
	{
		return this.abilityHandler != null && this.abilityHandler.UseAbility();
	}

	public bool IsAbilityUse()
	{
		return this.abilityHandler != null && this.abilityHandler.IsAbilityUse();
	}

	public void ResetOnMeetingStart()
	{
		this.abilityHandler?.ResetOnMeetingStart();
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
		this.abilityHandler?.ResetOnMeetingEnd(exiledPlayer);
	}

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer != null)
		{
			this.abilityHandler?.UpdateVisuals(rolePlayer.PlayerId);
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateIntOption(
			EncloserOption.StakeCount,
			3, 3, 10, 1);
		factory.CreateIntOption(
			EncloserOption.MetsuLimit,
			1, 1, 10, 1);
		factory.CreateFloatOption(
			EncloserOption.AbilityCoolTime,
			20.0f, 5.0f, 60.0f, 2.5f);
		factory.CreateIntOption(
			EncloserOption.MetsuKillMoney,
			10, 0, 100, 1);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);

		this.stakeCount = loader.GetValue<EncloserOption, int>(EncloserOption.StakeCount);
		this.metsuLimit = loader.GetValue<EncloserOption, int>(EncloserOption.MetsuLimit);
		this.abilityCoolTime = loader.GetValue<EncloserOption, float>(EncloserOption.AbilityCoolTime);
		this.metsuKillMoney = loader.GetValue<EncloserOption, int>(EncloserOption.MetsuKillMoney);
	}

	public static void RpcOps(MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		EncloserRpcOpsType ops = (EncloserRpcOpsType)reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<Encloser>(rolePlayerId, out var encloser) ||
			encloser.abilityHandler == null)
		{
			return;
		}

		switch (ops)
		{
			case EncloserRpcOpsType.PlaceStake:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				encloser.abilityHandler.HandlePlaceStake(rolePlayerId, new Vector2(x, y));
				break;
			case EncloserRpcOpsType.UseMetsu:
				float moneyGained = reader.ReadSingle();
				int count = reader.ReadInt32();
				List<byte> targets = new List<byte>(count);
				for (int i = 0; i < count; i++)
				{
					targets.Add(reader.ReadByte());
				}

				encloser.abilityHandler.HandleUseMetsu(rolePlayerId, targets, moneyGained);
				break;
		}
	}

	public static bool IsPointInPolygon(Vector2 point, IReadOnlyList<Vector2> polygon)
	{
		if (polygon == null || polygon.Count < 3)
		{
			return false;
		}

		bool inside = false;
		float px = point.x;
		float py = point.y;

		int j = polygon.Count - 1;
		for (int i = 0; i < polygon.Count; i++)
		{
			float ix = polygon[i].x;
			float iy = polygon[i].y;
			float jx = polygon[j].x;
			float jy = polygon[j].y;

			bool intersect = ((iy > py) != (jy > py)) &&
				(px < (jx - ix) * (py - iy) / (jy - iy) + ix);

			if (intersect)
			{
				inside = !inside;
			}

			j = i;
		}

		return inside;
	}
}

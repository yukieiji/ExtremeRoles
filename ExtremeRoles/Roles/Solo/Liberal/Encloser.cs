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
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;

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

public sealed class EncloserPolygon
{
	public bool IsCompleted { get; private set; }
	public int Count => this.stakes.Count;
	public IReadOnlyList<Vector2> Stakes => this.stakes;

	private readonly List<Vector2> stakes = new List<Vector2>();
	private readonly List<GameObject> stakeObjects = new List<GameObject>();
	private readonly IGameObjectFactory factory;

	private GameObject? linesObj;
	private LineRenderer? lineRenderer;
	private GameObject? meshObj;
	private MeshFilter? meshFilter;
	private MeshRenderer? meshRenderer;

	public EncloserPolygon(IGameObjectFactory? factory = null)
	{
		this.factory = factory ?? new DefaultGameObjectFactory();
	}

	public void AddStake(Vector2 pos, int maxStakes)
	{
		this.stakes.Add(pos);

		var stake = this.factory.Create($"EncloserStake_{this.stakes.Count}");
		if (stake != null)
		{
			var sr = stake.AddComponent<SpriteRenderer>();
			if (sr != null)
			{
				sr.sprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);
				sr.color = ColorPalette.LiberalColor;
			}

			stake.transform.position = new Vector3(pos.x, pos.y, -0.1f);
			this.stakeObjects.Add(stake);
		}

		if (this.stakes.Count >= maxStakes)
		{
			this.IsCompleted = true;
		}

		rebuildLinesAndMesh();
	}

	public bool IsPointInside(Vector2 point)
	{
		if (!this.IsCompleted || this.stakes.Count < 3)
		{
			return false;
		}

		bool inside = false;
		float px = point.x;
		float py = point.y;

		int j = this.stakes.Count - 1;
		for (int i = 0; i < this.stakes.Count; i++)
		{
			float ix = this.stakes[i].x;
			float iy = this.stakes[i].y;
			float jx = this.stakes[j].x;
			float jy = this.stakes[j].y;

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

	public void UpdateVisuals(bool isLocalPlayerEncloser)
	{
		bool showVisuals = this.IsCompleted || isLocalPlayerEncloser;

		if (this.linesObj != null)
		{
			this.linesObj.SetActive(showVisuals);
		}

		if (this.meshObj != null)
		{
			this.meshObj.SetActive(showVisuals);
		}
	}

	public void Clear()
	{
		foreach (var stake in this.stakeObjects)
		{
			if (stake != null)
			{
				Object.Destroy(stake);
			}
		}

		this.stakeObjects.Clear();

		if (this.linesObj != null)
		{
			Object.Destroy(this.linesObj);
			this.linesObj = null;
			this.lineRenderer = null;
		}

		if (this.meshObj != null)
		{
			Object.Destroy(this.meshObj);
			this.meshObj = null;
			this.meshFilter = null;
			this.meshRenderer = null;
		}

		this.stakes.Clear();
		this.IsCompleted = false;
	}

	private void rebuildLinesAndMesh()
	{
		if (this.stakes.Count < 2)
		{
			return;
		}

		if (this.linesObj == null)
		{
			this.linesObj = this.factory.Create("EncloserLines");
			if (this.linesObj != null)
			{
				this.lineRenderer = this.linesObj.AddComponent<LineRenderer>();
				if (this.lineRenderer != null)
				{
					this.lineRenderer.startWidth = 0.15f;
					this.lineRenderer.endWidth = 0.15f;
					this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
					this.lineRenderer.startColor = ColorPalette.LiberalColor;
					this.lineRenderer.endColor = ColorPalette.LiberalColor;
				}
			}
		}

		if (this.lineRenderer != null)
		{
			int posCount = this.stakes.Count + (this.IsCompleted ? 1 : 0);
			this.lineRenderer.positionCount = posCount;

			for (int i = 0; i < this.stakes.Count; i++)
			{
				this.lineRenderer.SetPosition(i, new Vector3(this.stakes[i].x, this.stakes[i].y, -0.05f));
			}

			if (this.IsCompleted)
			{
				this.lineRenderer.SetPosition(this.stakes.Count, new Vector3(this.stakes[0].x, this.stakes[0].y, -0.05f));
				buildMesh();
			}
		}
	}

	private void buildMesh()
	{
		if (this.stakes.Count < 3)
		{
			return;
		}

		if (this.meshObj == null)
		{
			this.meshObj = this.factory.Create("EncloserPolygonMesh");
			if (this.meshObj != null)
			{
				this.meshFilter = this.meshObj.AddComponent<MeshFilter>();
				this.meshRenderer = this.meshObj.AddComponent<MeshRenderer>();
				if (this.meshRenderer != null)
				{
					this.meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
				}

				this.meshObj.AddComponent<EncloserPolygonMeshBehaviour>();
			}
		}

		if (this.meshFilter != null)
		{
			Mesh mesh = new Mesh();
			Vector3[] vertices = new Vector3[this.stakes.Count];
			for (int i = 0; i < this.stakes.Count; i++)
			{
				vertices[i] = new Vector3(this.stakes[i].x, this.stakes[i].y, 0.05f);
			}

			int triangleCount = (this.stakes.Count - 2) * 3;
			int[] triangles = new int[triangleCount];
			int tIndex = 0;
			for (int i = 1; i < this.stakes.Count - 1; i++)
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

			this.meshFilter.mesh = mesh;
		}
	}
}

public sealed class EncloserAbilityHandler : IAbility, IRoleAutoBuildAbility, IRoleResetMeeting
{
	public ExtremeAbilityButton Button { get; set; } = null!;

	public EncloserMode CurrentMode => this.modeSwitcher?.Current ?? EncloserMode.Stake;

	public EncloserPolygon Polygon => this.polygon;

	private readonly EncloserPolygon polygon;
	private readonly int stakeCount;
	private readonly int metsuLimit;
	private readonly float abilityCoolTime;
	private readonly int metsuKillMoney;

	private int remainingMetsuCount;
	private GraphicSwitcher<EncloserMode>? modeSwitcher;

	public EncloserAbilityHandler(
		int stakeCount,
		int metsuLimit,
		float abilityCoolTime,
		int metsuKillMoney,
		IGameObjectFactory? factory = null)
	{
		this.polygon = new EncloserPolygon(factory);
		this.stakeCount = stakeCount;
		this.metsuLimit = metsuLimit;
		this.abilityCoolTime = abilityCoolTime;
		this.metsuKillMoney = metsuKillMoney;
		this.remainingMetsuCount = metsuLimit;
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
				caller.WriteFloat(pos.x);
				caller.WriteFloat(pos.y);
			}

			HandlePlaceStake(PlayerControl.LocalPlayer.PlayerId, pos);
			return true;
		}
		else if (this.CurrentMode == EncloserMode.Metsu)
		{
			byte encloserPlayerId = PlayerControl.LocalPlayer.PlayerId;
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
				if (!this.polygon.IsPointInside(pPos))
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

				Player.RpcUncheckMurderPlayer(encloserPlayerId, pc.PlayerId, byte.MinValue);
			}

			if (nonLiberalKills > 0)
			{
				float totalMoney = nonLiberalKills * this.metsuKillMoney;
				LiberalMoneyBankSystem.RpcUpdateSystem(
					encloserPlayerId,
					LiberalMoneyHistory.Reason.AddOnKill,
					totalMoney,
					0.0f);
			}

			this.remainingMetsuCount--;
			this.polygon.Clear();

			if (this.remainingMetsuCount > 0)
			{
				this.modeSwitcher?.Switch(EncloserMode.Stake);
			}

			this.Button.Behavior.SetCoolTime(this.abilityCoolTime);
			return true;
		}

		return false;
	}

	public bool IsAbilityUse()
	{
		if (this.remainingMetsuCount <= 0)
		{
			return false;
		}

		if (this.CurrentMode == EncloserMode.Stake)
		{
			return IRoleAutoBuildAbility.IsCommonUse() &&
				this.polygon.Count < this.stakeCount;
		}
		else if (this.CurrentMode == EncloserMode.Metsu)
		{
			return IRoleAutoBuildAbility.IsCommonUse() &&
				this.polygon.IsCompleted;
		}

		return false;
	}

	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.polygon.AddStake(pos, this.stakeCount);

		if (this.polygon.IsCompleted)
		{
			if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId)
			{
				this.modeSwitcher?.Switch(EncloserMode.Metsu);
			}
		}

		this.polygon.UpdateVisuals(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId);
	}

	public void ResetOnMeetingStart()
	{
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void Reset()
	{
		this.polygon.Clear();
		if (PlayerControl.LocalPlayer != null && this.Button != null)
		{
			this.modeSwitcher?.Switch(EncloserMode.Stake);
		}
	}

	public void UpdateVisuals(byte encloserPlayerId)
	{
		bool isLocalPlayerEncloser = PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId;
		this.polygon.UpdateVisuals(isLocalPlayerEncloser);
	}
}

public enum EncloserRpcOpsType : byte
{
	PlaceStake,
	UseMetsu
}

public sealed class Encloser : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{
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

	private EncloserAbilityHandler? abilityHandler;

	private int stakeCount;
	private int metsuLimit;
	private float abilityCoolTime;
	private int metsuKillMoney;

	public Encloser() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Encloser))
	{
	}

	public override string GetRoleTag()
	{
		return "Ec";
	}

	public void CreateAbility()
	{
		this.abilityHandler = new EncloserAbilityHandler(
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
		float x = reader.ReadSingle();
		float y = reader.ReadSingle();

		if (ExtremeRoleManager.TryGetSafeCastedRole<Encloser>(rolePlayerId, out var encloser) &&
			encloser.abilityHandler != null)
		{
			encloser.abilityHandler.HandlePlaceStake(rolePlayerId, new Vector2(x, y));
		}
	}
}

using System.Collections.Generic;

using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

using ExtremeRoles.Core;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Liberal;

public sealed class EncloserPolygon(IUnityObjectFactory? factory = null, IIl2CppObjectProvider? il2cppProvider = null)
{
	public bool IsCompleted { get; private set; }
	public int Count => this.stakeObjects.Count;

	private readonly List<GameObject> stakeObjects = [];
	private readonly IUnityObjectFactory factory = factory ?? new DefaultUnityObjectFactory();
	private readonly IIl2CppObjectProvider il2cppProvider = il2cppProvider ?? new DefaultIl2CppObjectProvider();

	private LineRenderer? lineRenderer;
	private MeshFilter? meshFilter;

	public void AddStake(Vector2 pos, int maxStakes)
	{
		var stake = this.factory.CreateGameObject($"EncloserStake_{this.stakeObjects.Count + 1}");
		var sr = stake.AddComponent<SpriteRenderer>();
		sr.sprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);
		sr.color = ColorPalette.LiberalColor;
		stake.transform.position = this.factory.CreateMapPos(pos);
		this.stakeObjects.Add(stake);

		if (this.stakeObjects.Count >= maxStakes)
		{
			this.IsCompleted = true;
		}

		rebuildLinesAndMesh();
	}

	public bool IsPointInside(Vector2 point)
	{
		int count = this.stakeObjects.Count;
		if (!this.IsCompleted || count < 3)
		{
			return false;
		}

		bool inside = false;
		float px = point.x;
		float py = point.y;

		int j = count - 1;
		for (int i = 0; i < count; i++)
		{
			Vector3 posI = this.stakeObjects[i].transform.position;
			Vector3 posJ = this.stakeObjects[j].transform.position;
			float ix = posI.x;
			float iy = posI.y;
			float jx = posJ.x;
			float jy = posJ.y;

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
		setObjectActive(this.IsCompleted || isLocalPlayerEncloser);
	}

	public void Clear()
	{
		setObjectActive(false);

		foreach (var stake in this.stakeObjects)
		{
			Object.Destroy(stake);
		}

		this.stakeObjects.Clear();

		if (this.lineRenderer != null)
		{
			if (this.lineRenderer.gameObject != null)
			{
				Object.Destroy(this.lineRenderer.gameObject);
			}

			this.lineRenderer = null;
		}

		if (this.meshFilter != null)
		{
			if (this.meshFilter.gameObject != null)
			{
				Object.Destroy(this.meshFilter.gameObject);
			}

			this.meshFilter = null;
		}

		this.IsCompleted = false;
	}

	private void rebuildLinesAndMesh()
	{
		int count = this.stakeObjects.Count;
		if (count < 2)
		{
			return;
		}

		if (this.lineRenderer == null)
		{
			var linesObj = this.factory.CreateGameObject("EncloserLines");
			this.lineRenderer = linesObj.AddComponent<LineRenderer>();
			this.lineRenderer.material = this.factory.CreateSpriteMaterial();
			this.lineRenderer.startWidth = 0.15f;
			this.lineRenderer.endWidth = 0.15f;
			this.lineRenderer.startColor = ColorPalette.LiberalColor;
			this.lineRenderer.endColor = ColorPalette.LiberalColor;
		}

		this.lineRenderer.positionCount = count + (this.IsCompleted ? 1 : 0);

		// 線は杭より後ろに置きたいので
		for (int i = 0; i < count; i++)
		{
			Vector3 pos = this.stakeObjects[i].transform.position;
			var linePos = this.factory.CreateMapPos(pos, 975.0f);
			this.lineRenderer.SetPosition(i, linePos);
		}

		if (this.IsCompleted)
		{
			Vector3 firstPos = this.stakeObjects[0].transform.position;
			var firstLinePos = this.factory.CreateMapPos(firstPos, 975.0f);
			this.lineRenderer.SetPosition(count, firstLinePos);
			buildMesh();
		}
	}

	private void buildMesh()
	{
		int count = this.stakeObjects.Count;
		if (count < 3)
		{
			return;
		}

		if (this.meshFilter == null)
		{
			var meshObj = this.factory.CreateGameObject("EncloserPolygonMesh");
			this.meshFilter = meshObj.AddComponent<MeshFilter>();
			var meshRenderer = meshObj.AddComponent<MeshRenderer>();
			meshRenderer.material = this.factory.CreateSpriteMaterial();
			meshObj.AddComponent<EncloserPolygonMeshBehaviour>();
		}

		Mesh mesh = this.factory.CreateMesh();
		Il2CppStructArray<Vector3> vertices = this.il2cppProvider.GetStructArray<Vector3>(count);
		for (int i = 0; i < count; i++)
		{
			Vector3 pos = this.stakeObjects[i].transform.position;
			vertices[i] = this.factory.CreateMapPos(pos, 950.0f);
		}

		int triangleCount = (count - 2) * 3;
		Il2CppStructArray<int> triangles = this.il2cppProvider.GetStructArray<int>(triangleCount);
		int tIndex = 0;
		for (int i = 1; i < count - 1; i++)
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

		Il2CppStructArray<Color> colors = this.il2cppProvider.GetStructArray<Color>(count);
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

	private void setObjectActive(bool showVisuals)
	{
		if (this.lineRenderer != null && this.lineRenderer.gameObject != null)
		{
			this.lineRenderer.gameObject.SetActive(showVisuals);
		}

		if (this.meshFilter != null && this.meshFilter.gameObject != null)
		{
			this.meshFilter.gameObject.SetActive(showVisuals);
		}
	}
}

public sealed class EncloserStatusModel(
	int stakeCount,
	int metsuKillMoney,
	int metsuLimit,
	IUnityObjectFactory? factory = null,
	IIl2CppObjectProvider? il2cppProvider = null
) : IStatusModel
{
	public int StakeCount { get; } = stakeCount;
	public int MetsuKillMoney { get; } = metsuKillMoney;
	public int RemainingMetsuCount { get; set; } = metsuLimit;

	public int CurStakeCount => this.polygon.Count;
	public bool IsUseMetsu => this.polygon.IsCompleted;

	private readonly EncloserPolygon polygon = new EncloserPolygon(factory, il2cppProvider);

	public void Update()
	{
		this.polygon.UpdateVisuals(true);
	}

	public bool IsKillPosition(Vector2 pos)
		=> this.polygon.IsPointInside(pos);

	public void ClearStake()
	{
		this.polygon.Clear();
	}

	public void PlaceStake(Vector2 pos, bool isLocalPlayerEncloser)
	{
		this.polygon.AddStake(pos, this.StakeCount);
		this.polygon.UpdateVisuals(isLocalPlayerEncloser);
	}
}

public sealed class EncloserStakeAbilityhandler(
	EncloserStatusModel statusModel,
	ICountBehavior count,
	GraphicSwitcher<Encloser.Mode> switcher)
{
	private readonly EncloserStatusModel encloserStatus = statusModel;
	private readonly ICountBehavior count = count;
	private readonly GraphicSwitcher<Encloser.Mode> switcher = switcher;

	public void CleanUp()
	{
		if (!this.encloserStatus.IsUseMetsu)
		{
			return;
		}

		this.switcher.Switch(Encloser.Mode.Metsu);
		this.count.SetAbilityCount(this.encloserStatus.RemainingMetsuCount);
	}

	public bool Invoke()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte encloserPlayerId = localPlayer.PlayerId;

		Vector2 pos = localPlayer.GetTruePosition();
		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
		{
			caller.WriteByte(encloserPlayerId);
			caller.WriteByte((byte)Encloser.RpcOpsType.PlaceStake);
			caller.WriteFloat(pos.x);
			caller.WriteFloat(pos.y);
		}

		HandlePlaceStake(encloserPlayerId, pos);

		return true;
	}
	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.encloserStatus.PlaceStake(pos, isLocalPlayerIsEncloser(encloserPlayerId));
	}

	private static bool isLocalPlayerIsEncloser(byte encloserPlayerId)
		=> PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.PlayerId == encloserPlayerId;
}

public sealed class EncloserMetsuAbilityhandler(
	EncloserStatusModel statusModel,
	ICountBehavior count,
	GraphicSwitcher<Encloser.Mode> switcher)
{
	private readonly EncloserStatusModel encloserStatus = statusModel;
	private readonly ICountBehavior count = count;
	private readonly GraphicSwitcher<Encloser.Mode> switcher = switcher;

	public void CleanUp()
	{
		this.encloserStatus.RemainingMetsuCount = count.AbilityCount;
		if (this.encloserStatus.RemainingMetsuCount <= 0)
		{
			return;
		}
		this.switcher.Switch(Encloser.Mode.Stake);
		count.SetAbilityCount(this.encloserStatus.StakeCount);
	}

	public bool Invoke()
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte encloserPlayerId = localPlayer.PlayerId;

		int nonLiberalKills = 0;

		foreach (var pc in PlayerCache.AllPlayerControl)
		{
			if (pc.IsInValid())
			{
				continue;
			}

			Vector2 pPos = pc.GetTruePosition();

			if (!this.encloserStatus.IsKillPosition(pPos) ||
				!ExtremeRoleManager.TryGetRole(pc.PlayerId, out var targetRole) ||
				targetRole.Core.Id is ExtremeRoleId.Leader ||
				(
					targetRole.AbilityClass is IInvincible invincible &&
					invincible.IsBlockKillFrom(encloserPlayerId)
				))
			{
				continue;
			}

			if (!targetRole.IsLiberal())
			{
				nonLiberalKills++;
			}

			Player.RpcUncheckMurderPlayer(encloserPlayerId, pc.PlayerId, byte.MinValue);
		}

		if (nonLiberalKills > 0)
		{
			float totalMoney = nonLiberalKills * this.encloserStatus.MetsuKillMoney;
			LiberalMoneyBankSystem.RpcUpdateSystem(
				encloserPlayerId,
				LiberalMoneyHistory.Reason.AddOnKill,
				totalMoney,
				0.0f);
		}

		using (var caller = RPCOperator.CreateCaller(RPCOperator.Command.EncloserOps))
		{
			caller.WriteByte((byte)Encloser.RpcOpsType.UseMetsu);
		}
		HandleUseMetsuRpc();
		return true;
	}

	public void HandleUseMetsuRpc()
	{
		this.encloserStatus.ClearStake();
	}

}

public sealed class EncloserAbilityHandler(
	EncloserStatusModel statusModel) : IAbility
{
	public ExtremeAbilityButton Button { get; set; } = null!;
	private EncloserStatusModel encloserStatus = statusModel;
	private Encloser.Mode currentMode => this.modeSwitcher?.Current ?? Encloser.Mode.Stake;
	private GraphicSwitcher<Encloser.Mode>? modeSwitcher;
	private EncloserStakeAbilityhandler? stake;
	private EncloserMetsuAbilityhandler? mestu;

	public void CreateAbility()
	{
		Sprite bombSprite = UnityObjectLoader.LoadSpriteFromResources(ObjectPath.Bomb);

		var stakeGraphic = new ButtonGraphic(Tr.GetString("Stake"), bombSprite);
		var metsuGraphic = new ButtonGraphic(Tr.GetString("Metsu"), bombSprite);

		this.Button = RoleAbilityFactory.CreateCountAbility(
			stakeGraphic.Text,
			stakeGraphic.Img,
			this.IsAbilityUse,
			this.UseAbility);

		this.Button.SetLabelToCrewmate();

		this.modeSwitcher = new GraphicSwitcher<Encloser.Mode>(
			this.Button.Behavior,
			new GraphicMode<Encloser.Mode>(Encloser.Mode.Stake, stakeGraphic),
			new GraphicMode<Encloser.Mode>(Encloser.Mode.Metsu, metsuGraphic));

		this.modeSwitcher.Switch(Encloser.Mode.Stake);

		if (this.Button.Behavior is ICountBehavior countBehavior)
		{
			this.stake = new EncloserStakeAbilityhandler(this.encloserStatus, countBehavior, this.modeSwitcher);
			this.mestu = new EncloserMetsuAbilityhandler(this.encloserStatus, countBehavior, this.modeSwitcher);
		}
	}

	public bool UseAbility()
		=> this.currentMode switch
		{ 
			Encloser.Mode.Stake => this.stake is not null && this.stake.Invoke(),
			Encloser.Mode.Metsu => this.mestu is not null && this.mestu.Invoke(),
			_ => false
		};

	public bool IsAbilityUse()
	{
		if (this.encloserStatus.RemainingMetsuCount <= 0)
		{
			return false;
		}

		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		bool isCommonUse = localPlayer != null && localPlayer.IsAlive() && localPlayer.CanMove;

		return this.currentMode switch
		{
			Encloser.Mode.Stake => isCommonUse && this.encloserStatus.CurStakeCount < this.encloserStatus.StakeCount,
			Encloser.Mode.Metsu => isCommonUse && this.encloserStatus.IsUseMetsu,
			_ => false
		};
	}

	public void AbilityOff()
	{
		switch (this.currentMode)
		{
			case Encloser.Mode.Stake:
				this.stake?.CleanUp();
				break;
			case Encloser.Mode.Metsu:
				this.mestu?.CleanUp();
				break;
			default:
				break;
		}
	}

	public void HandlePlaceStake(byte encloserPlayerId, Vector2 pos)
	{
		this.stake?.HandlePlaceStake(encloserPlayerId, pos);
	}

	public void HandleUseMetsuRpc()
	{
		this.mestu?.HandleUseMetsuRpc();
	}
}

public sealed class Encloser : SingleRoleBase, IRoleAutoBuildAbility, IRoleUpdate, IRoleResetMeeting
{

	public enum Option
	{
		StakeCount,
		MetsuLimit,
		AbilityCoolTime,
		MetsuKillMoney
	}

	public enum Mode : byte
	{
		Stake,
		Metsu
	}

	public enum RpcOpsType : byte
	{
		PlaceStake,
		UseMetsu
	}

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

	public override IStatusModel? Status => this.status;

	private EncloserStatusModel? status;
	private EncloserAbilityHandler? abilityHandler;

	public Encloser() : base(
		RoleArgs.BuildLiberalMilitant(ExtremeRoleId.Encloser))
	{
	}

	public override string GetRoleTag()
		=> "Ec";

	public void CreateAbility()
	{
		var loader = this.Loader;
		int stakeCount = loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount);
		int metsuLimit = loader.GetValue<Option, int>(Option.MetsuLimit);
		float abilityCoolTime = loader.GetValue<Option, float>(Option.AbilityCoolTime);
		int metsuKillMoney = loader.GetValue<Option, int>(Option.MetsuKillMoney);

		this.status = new EncloserStatusModel(stakeCount, metsuKillMoney, metsuLimit);
		this.abilityHandler = new EncloserAbilityHandler(this.status);
		
		this.AbilityClass = this.abilityHandler;

		this.abilityHandler?.CreateAbility();
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
	}

	public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
	{
	}

	public void Update(PlayerControl rolePlayer)
	{
		this.status?.Update();
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		IRoleAbility.CreateAbilityCountOption(factory, 3, 3, 10, 1);
		factory.CreateIntOption(
			Option.MetsuLimit,
			1, 1, 10, 1);
		factory.CreateFloatOption(
			Option.AbilityCoolTime,
			20.0f, 5.0f, 60.0f, 2.5f);
		factory.CreateIntOption(
			Option.MetsuKillMoney,
			10, 0, 100, 1);
	}

	protected override void RoleSpecificInit()
	{
		var liberalOption = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<LiberalDefaultOptionLoader>();
		LiberalSettingOverrider.OverrideDefault(this, liberalOption);
	}

	public static void RpcOps(MessageReader reader)
	{
		byte rolePlayerId = reader.ReadByte();
		RpcOpsType ops = (RpcOpsType)reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<Encloser>(rolePlayerId, out var encloser) ||
			encloser.abilityHandler == null)
		{
			return;
		}

		switch (ops)
		{
			case RpcOpsType.PlaceStake:
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				encloser.abilityHandler.HandlePlaceStake(rolePlayerId, new Vector2(x, y));
				break;
			case RpcOpsType.UseMetsu:
				encloser.abilityHandler.HandleUseMetsuRpc();
				break;
		}
	}
}

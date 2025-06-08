﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Hazel;
using AmongUs.GameOptions;

using Newtonsoft.Json.Linq;

using BepInEx.Unity.IL2CPP.Utils;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Resources;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.GameMode;
using ExtremeRoles.Patches;


using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.GameResult;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Hypnotist :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleAwake<RoleTypes>,
    IRoleMurderPlayerHook,
    IRoleSpecialReset
{
    public enum HypnotistOption
    {
        AwakeCheckImpostorNum,
        AwakeCheckTaskGage,
        AwakeKillCount,
        Range,
        HideArrowRange,
        DefaultRedAbilityPart,
        HideKillButtonTime,
        DollKillCoolReduceRate,
        IsResetKillCoolWhenDollKill,
        DollCrakingCoolTime,
        DollCrakingActiveTime
    }

    public enum RpcOps : byte
    {
        TargetToDoll,
        PickUpAbilityModule,
        ResetDollKillButton,
    }

    public enum AbilityModuleType : byte
    {
        Red,
        Blue,
        Gray
    }

    public bool IsAwake
    {
        get
        {
            return GameSystem.IsLobby || this.isAwake;
        }
    }

    public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

    private float dollKillCoolReduceRate;

    private bool isResetKillCoolWhenDollKill;
    private int defaultRedAbilityPartNum;

    private int awakeCheckImpNum;
    private float awakeCheckTaskGage;

    private bool isAwake;
    private bool canAwakeNow;
    private int killCount;
    private int awakeKillCount;

    private bool isAwakedHasOtherVision;
    private bool isAwakedHasOtherKillCool;
    private bool isAwakedHasOtherKillRange;

    private float defaultKillCool;
    private float range;

    private PlayerControl? target;

    private const string adminKey = "Admin";
    private const string securityKey = "Security";
    private const string vitalKey = "Vital";

    private int addRedPosNum;

    private float hideDistance = 7.5f;

    public float DollCrakingCoolTime { get; private set; }
    public float DollCrakingActiveTime { get; private set; }

    private bool isActiveTimer;
    private float timer;
    private float defaultTimer;

#pragma warning disable CS8618
	private List<Vector3> addedPos;
	private List<Vector3> addRedPos;
	public ExtremeAbilityButton Button { get; set; }
	private HashSet<byte> doll;

	private JObject position;
	private const string postionJson =
		"ExtremeRoles.Resources.JsonData.HypnotistAbilityPartPosition.json";

	public Hypnotist() : base(
        ExtremeRoleId.Hypnotist,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Hypnotist.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }
#pragma warning restore CS8618

	public static void Ability(ref MessageReader reader)
    {
        byte rolePlayerId = reader.ReadByte();
        Hypnotist? role = ExtremeRoleManager.GetSafeCastedRole<Hypnotist>(rolePlayerId);
        RpcOps ops = (RpcOps)reader.ReadByte();
        switch (ops)
        {
            case RpcOps.TargetToDoll:
                byte targetPlayerId = reader.ReadByte();
				if (role is null)
				{
					return;
				}
                targetToDoll(role, rolePlayerId, targetPlayerId);
                break;
            case RpcOps.PickUpAbilityModule:
				if (role is null)
				{
					return;
				}
				updateDoll(role, ref reader);
                break;
            case RpcOps.ResetDollKillButton:
				if (role is null)
				{
					return;
				}
				resetDollKillButton(role);
                break;
        }
    }

    public static void UpdateAllDollKillButtonState(Hypnotist role)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        float optionKillCool = Player.DefaultKillCoolTime;

        foreach (byte dollPlayerId in role.doll)
        {
            SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
            if (doll.Id == ExtremeRoleId.Doll)
            {
                float curKillCool = localPlayer.killTimer;
                if (localPlayer.PlayerId == dollPlayerId &&
                    doll.CanKill &&
                    curKillCool > 0.0f)
                {
                    localPlayer.killTimer = Mathf.Clamp(
                        curKillCool * role.dollKillCoolReduceRate,
                        0.001f, optionKillCool);
                }
                doll.CanKill = true;
            }
        }
    }

    public static void FeatAllDollMapModuleAccess(
        Hypnotist role, SystemConsoleType console)
    {
        Logging.Debug($"FeatAccess:{console}");
        foreach (byte dollPlayerId in role.doll)
        {
            SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
            if (doll is Doll castedDoll)
            {
                castedDoll.FeatMapModuleAccess(console);
            }
        }
    }

    public static void UnlockAllDollCrakingAbility(
        Hypnotist role, SystemConsoleType unlockConsole)
    {
        Logging.Debug($"unlock:{unlockConsole}");
        foreach (byte dollPlayerId in role.doll)
        {
            SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
            if (doll is Doll castedDoll)
            {
                castedDoll.UnlockCrakingAbility(unlockConsole);
            }
        }
    }

    private static void targetToDoll(
        Hypnotist role,
        byte rolePlayerId,
        byte targetPlayerId)
    {

        PlayerControl targetPlayer = Player.GetPlayerControlById(targetPlayerId);
        SingleRoleBase targetRole = ExtremeRoleManager.GameRole[targetPlayerId];

        IRoleSpecialReset.ResetRole(targetPlayerId);
        Doll newDoll = new Doll(targetPlayerId, rolePlayerId, role);
        if (targetPlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            newDoll.CreateAbility();
        }
        ExtremeRoleManager.SetNewRole(targetPlayerId, newDoll);

        if (targetRole.Id == ExtremeRoleId.Lover)
        {
            targetRole.RolePlayerKilledAction(targetPlayer, targetPlayer);
        }
        role.doll.Add(targetPlayerId);
    }

    private static void updateDoll(
        Hypnotist role,
        ref MessageReader reader)
    {
        AbilityModuleType type = (AbilityModuleType)reader.ReadByte();
        switch (type)
        {
            case AbilityModuleType.Red:
                UpdateAllDollKillButtonState(role);
                break;
            case AbilityModuleType.Blue:
                SystemConsoleType featAbilityConsole = (SystemConsoleType)reader.ReadByte();
                FeatAllDollMapModuleAccess(role, featAbilityConsole);
                break;
            case AbilityModuleType.Gray:
                SystemConsoleType unlockConsole = (SystemConsoleType)reader.ReadByte();
                UnlockAllDollCrakingAbility(role, unlockConsole);
                break;
            default:
                break;
        }
    }

    private static void resetDollKillButton(Hypnotist role)
    {
        foreach (byte dollPlayerId in role.doll)
        {
            SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
            if (doll.Id == ExtremeRoleId.Doll)
            {
                doll.CanKill = false;
            }
        }
    }

    public void EnableKillTimer()
    {
        this.timer = this.defaultTimer;
        this.isActiveTimer = true;
    }

    public void RemoveDoll(byte playerId)
    {
        this.doll.Remove(playerId);
    }

    public void RemoveAbilityPartPos(Vector3 pos)
    {
        this.addedPos.Remove(pos);
    }

    public string GetFakeOptionString() => "";

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "Hypnosis",
			UnityObjectLoader.LoadFromResources(ExtremeRoleId.Hypnotist));

		var json = JsonParser.GetJObjectFromAssembly(postionJson);
		if (json == null)
		{
			throw new InvalidOperationException("Can't find json file");
		}

		this.position = json;

	}

    public bool IsAbilityUse()
    {
        if (!this.IsAwake) { return false; }

        this.target = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, this.range);

        return this.target != null && IRoleAbility.IsCommonUse();
    }

    public void ResetOnMeetingStart()
    {
        if (this.isAwake && this.doll.Count > 0)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.HypnotistAbility))
            {
                caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte((byte)RpcOps.ResetDollKillButton);
            }
            resetDollKillButton(this);
        }
        this.isActiveTimer = false;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        updateAwakeCheck(exiledPlayer);

		if (!this.isAwake &&
            this.canAwakeNow &&
            this.killCount >= this.awakeKillCount)
        {
            this.isAwake = true;
            this.HasOtherVision = this.isAwakedHasOtherVision;
            this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
            this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            this.Button?.SetButtonShow(true);
        }
        if (this.isAwake && this.addRedPos.Count > 0)
        {
            setRedAbilityPart(this.addRedPos.Count);
        }
    }

    public bool UseAbility()
    {
        PlayerControl rolePlayer = PlayerControl.LocalPlayer;
        byte targetPlayerId = this.target!.PlayerId;

        SingleRoleBase role = ExtremeRoleManager.GameRole[targetPlayerId];

        int redPartNum = this.defaultRedAbilityPartNum;
        Type roleType = role.GetType();
        Type[] interfaces = roleType.GetInterfaces();

        redPartNum += computeRedPartNum(interfaces);

        if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole != null)
        {
			Type anotherRoleType = multiAssignRole.AnotherRole.GetType();
			Type[] anotherInterface = anotherRoleType.GetInterfaces();
			redPartNum += computeRedPartNum(anotherInterface);
		}

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.HypnotistAbility))
        {
            caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
            caller.WriteByte((byte)RpcOps.TargetToDoll);
            caller.WriteByte(targetPlayerId);
        }
        targetToDoll(this, rolePlayer.PlayerId, targetPlayerId);
        setAbilityPart(redPartNum);
        this.target = null;

        return true;
    }

    public void Update(PlayerControl rolePlayer)
    {
        // 追放中は処理をブロックする
        if (rolePlayer == null ||
            ExileController.Instance) { return; }

        if (!this.canAwakeNow)
        {
            updateAwakeCheck(null);
        }
        if (!this.isAwake &&
            this.Button != null)
        {
            this.Button.SetButtonShow(false);
        }

        if (this.isActiveTimer)
        {
            this.timer -= Time.deltaTime;
            if (this.timer <= 0.0f)
            {
                Logging.Debug("ResetKillButton");
                this.isActiveTimer = false;
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.HypnotistAbility))
                {
                    caller.WriteByte(rolePlayer.PlayerId);
                    caller.WriteByte((byte)RpcOps.ResetDollKillButton);
                }
                resetDollKillButton(this);
            }
        }
    }

    public void HookMuderPlayer(
        PlayerControl source, PlayerControl target)
    {
        if (this.doll.Contains(source.PlayerId) &&
            this.isResetKillCoolWhenDollKill)
        {
            PlayerControl.LocalPlayer.killTimer = this.defaultKillCool;
        }
    }

    public void AllReset(PlayerControl rolePlayer)
    {
        foreach (byte playerId in this.doll)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null) { continue; }

            if (player.Data.IsDead ||
                player.Data.Disconnected) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    public override string GetColoredRoleName(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetColoredRoleName();
        }
        else
        {
            return Design.ColoedString(
                Palette.ImpostorRed, Tr.GetString(RoleTypes.Impostor.ToString()));
        }
    }
    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return Tr.GetString(
                $"{this.Id}FullDescription");
        }
        else
        {
            return Tr.GetString(
                $"{RoleTypes.Impostor}FullDescription");
        }
    }

    public override string GetImportantText(bool isContainFakeTask = true)
    {
        if (IsAwake)
        {
            return base.GetImportantText(isContainFakeTask);

        }
        else
        {
            return string.Concat(new string[]
            {
                TranslationController.Instance.GetString(
                    StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                "\r\n",
                Palette.ImpostorRed.ToTextColor(),
                TranslationController.Instance.GetString(
                    StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                "</color>"
            });
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return base.GetIntroDescription();
        }
        else
        {
            return Design.ColoedString(
                Palette.ImpostorRed,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
        }
    }

    public override Color GetNameColor(bool isTruthColor = false)
    {
        if (isTruthColor || IsAwake)
        {
            return base.GetNameColor(isTruthColor);
        }
        else
        {
            return Palette.ImpostorRed;
        }
    }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (!this.isAwake)
        {
            ++this.killCount;
        }
        return true;
    }

    public override void ExiledAction(PlayerControl rolePlayer)
    {
        foreach (byte playerId in this.doll)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null) { continue; }
            if (player.Data.IsDead || player.Data.Disconnected) { continue; }

            player.Exiled();
        }
    }

    public override void RolePlayerKilledAction(
        PlayerControl rolePlayer, PlayerControl killerPlayer)
    {
        foreach (byte playerId in this.doll)
        {
            PlayerControl player = Player.GetPlayerControlById(playerId);

            if (player == null) { continue; }

            if (player.Data.IsDead ||
                player.Data.Disconnected) { continue; }

            RPCOperator.UncheckedMurderPlayer(
                playerId, playerId,
                byte.MaxValue);
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateIntOption(
            HypnotistOption.AwakeCheckImpostorNum,
            1, 1, GameSystem.MaxImposterNum, 1);
        factory.CreateIntOption(
            HypnotistOption.AwakeCheckTaskGage,
            60, 0, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateIntOption(
            HypnotistOption.AwakeKillCount,
            2, 0, 5, 1,
            format: OptionUnit.Shot);

        factory.CreateFloatOption(
            HypnotistOption.Range,
            1.6f, 0.5f, 5.0f, 0.1f);

        IRoleAbility.CreateAbilityCountOption(factory, 1, 5);

        factory.CreateFloatOption(
            HypnotistOption.HideArrowRange,
            10.0f, 5.0f, 25.0f, 0.5f);
        factory.CreateIntOption(
            HypnotistOption.DefaultRedAbilityPart,
            0, 0, 10, 1);
        factory.CreateFloatOption(
            HypnotistOption.HideKillButtonTime,
            15.0f, 2.5f, 60.0f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateIntOption(
            HypnotistOption.DollKillCoolReduceRate,
            10, 0, 75, 1,
            format: OptionUnit.Percentage);

        factory.CreateBoolOption(
            HypnotistOption.IsResetKillCoolWhenDollKill,
            true);
        factory.CreateFloatOption(
            HypnotistOption.DollCrakingCoolTime,
            30.0f, 0.5f, 120.0f, 0.5f,
            format: OptionUnit.Second);
        factory.CreateFloatOption(
            HypnotistOption.DollCrakingActiveTime,
            3.0f, 0.5f, 60.0f, 0.5f,
            format: OptionUnit.Second);

    }

    protected override void RoleSpecificInit()
    {
        var curOption = GameOptionsManager.Instance.CurrentGameOptions;

        this.defaultKillCool = Player.DefaultKillCoolTime;

        if (this.HasOtherKillCool)
        {
            this.defaultKillCool = this.KillCoolTime;
        }

        var cate = this.Loader;
        this.awakeCheckImpNum = cate.GetValue<HypnotistOption, int>(
            HypnotistOption.AwakeCheckImpostorNum);
        this.awakeCheckTaskGage = cate.GetValue<HypnotistOption, int>(
            HypnotistOption.AwakeCheckTaskGage) / 100.0f;
        this.awakeKillCount = cate.GetValue<HypnotistOption, int>(
            HypnotistOption.AwakeKillCount);

        this.range = cate.GetValue<HypnotistOption, float>(
            HypnotistOption.Range);

        this.hideDistance = cate.GetValue<HypnotistOption, float>(
            HypnotistOption.HideArrowRange);
        this.isResetKillCoolWhenDollKill = cate.GetValue<HypnotistOption, bool>(
            HypnotistOption.IsResetKillCoolWhenDollKill);
        this.dollKillCoolReduceRate = (1.0f - (cate.GetValue<HypnotistOption, int>(
            HypnotistOption.DollKillCoolReduceRate) / 100.0f));
        this.defaultRedAbilityPartNum = cate.GetValue<HypnotistOption, int>(
            HypnotistOption.DefaultRedAbilityPart);

        this.DollCrakingActiveTime = cate.GetValue<HypnotistOption, float>(
            HypnotistOption.DollCrakingActiveTime);
        this.DollCrakingCoolTime = cate.GetValue<HypnotistOption, float>(
            HypnotistOption.DollCrakingCoolTime);

        this.defaultTimer = cate.GetValue<HypnotistOption, float>(
            HypnotistOption.HideKillButtonTime);

        this.canAwakeNow =
            this.awakeCheckImpNum >= curOption.GetInt(Int32OptionNames.NumImpostors) &&
            this.awakeCheckTaskGage <= 0.0f;

        this.killCount = 0;

        this.doll = new HashSet<byte>();

        this.isAwakedHasOtherVision = false;
        this.isAwakedHasOtherKillCool = true;
        this.isAwakedHasOtherKillRange = false;

        if (this.HasOtherVision)
        {
            this.HasOtherVision = false;
            this.isAwakedHasOtherVision = true;
        }

        if (this.HasOtherKillCool)
        {
            this.HasOtherKillCool = false;
        }

        if (this.HasOtherKillRange)
        {
            this.HasOtherKillRange = false;
            this.isAwakedHasOtherKillRange = true;
        }

        if (this.canAwakeNow && this.awakeKillCount <= 0)
        {
            this.isAwake = true;
            this.HasOtherVision = this.isAwakedHasOtherVision;
            this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
            this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
        }
        this.doll = new HashSet<byte>();
        this.addedPos = new List<Vector3>();
        this.addRedPos = new List<Vector3>();
        this.addRedPosNum = 0;

        this.isActiveTimer = false;
    }

	private void addJsonValeToConsoleType(in JObject json, in List<(Vector3, SystemConsoleType)> result, in string key, SystemConsoleType type)
	{
		if (!json.TryGetValue(key, out JToken token))
		{
			return;
		}
		JArray? pos = token.TryCast<JArray>();
		if (pos == null) { return; }
		Vector3 vecPos = new Vector3(
			(float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));

		if (this.addedPos.Contains(vecPos))
		{
			return;
		}

		result.Add((vecPos, type));
	}

	private IReadOnlyList<(Vector3, SystemConsoleType)> getSystemConsolePartPos(in JToken json, in string key)
	{
		List<(Vector3, SystemConsoleType)> result = new List<(Vector3, SystemConsoleType)>();

		JObject? keyJson = json.Get<JObject>(key);
		if (keyJson == null) { return result; }

		addJsonValeToConsoleType(keyJson, result, adminKey, SystemConsoleType.AdminModule);
		addJsonValeToConsoleType(keyJson, result, securityKey, SystemConsoleType.SecurityCamera);
		addJsonValeToConsoleType(keyJson, result, vitalKey, SystemConsoleType.VitalsLabel);

		return result;
	}

	private void setAbilityPart(int redModuleNum)
    {
		string key = Map.Name;
		setAbilityPartFromMapJsonInfo(this.position[key], redModuleNum);
	}
    private void setAbilityPartFromMapJsonInfo(
        JToken json, int redNum)
    {
		var redPos = getRedPartPos(json);
		this.addRedPosNum = redPos.Count;

		var bluePos = getSystemConsolePartPos(json, "Blue");
		var grayPos = getSystemConsolePartPos(json, "Gray");

        List<Vector3> noneSortedAddPos = new List<Vector3>();

        for (int i = 0; i < redNum; ++i)
        {
            int useIndex = i % redPos.Count;
            noneSortedAddPos.Add(redPos[useIndex]);
        }

        this.addRedPos = noneSortedAddPos.OrderBy(
            x => RandomGenerator.Instance.Next()).ToList();
        setRedAbilityPart(redNum);

        foreach (var (pos, console) in grayPos)
        {
			setParts<GrayAbilityPart>(pos)
				.SetConsoleType(console);
		}
        foreach (var (pos, console) in bluePos)
        {
			setParts<BlueAbilityPart>(pos)
				.SetConsoleType(console);
        }
    }

    private void setRedAbilityPart(int maxSetNum)
    {
        int setNum = Math.Min(this.addRedPosNum, maxSetNum);
        int checkIndex = 0;
        for (int i = 0; i < setNum; ++i)
        {
            Vector3 pos = this.addRedPos[checkIndex];

            if (this.addedPos.Contains(pos))
            {
                ++checkIndex;
                continue;
            }

			setParts<RedAbilityPart>(pos);

            this.addRedPos.RemoveAt(checkIndex);
            this.addedPos.Add(pos);
        }
    }
	private T setParts<T>(Vector3 pos) where T : AbilityPartBase
	{
		GameObject obj = new GameObject(nameof(T));
		obj.transform.position = pos;
		T abilityPart = obj.AddComponent<T>();
		abilityPart.SetHideArrowDistance(this.hideDistance);
		return abilityPart;
	}

    private void updateAwakeCheck(NetworkedPlayerInfo? ignorePlayer)
    {
        int impNum = 0;

        foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            byte playerId = player.PlayerId;

            if (ignorePlayer != null &&
                player.PlayerId == ignorePlayer.PlayerId) { continue; }

            if (ExtremeRoleManager.GameRole[playerId].IsImpostor() &&
                (!player.IsDead && !player.Disconnected))
            {
                ++impNum;
            }
        }

        GameData gameData = GameData.Instance;

		float compTask = gameData.CompletedTasks;
		float totalTask = gameData.TotalTasks;

		this.canAwakeNow = (
			this.awakeCheckImpNum >= impNum &&
			(
				GameDataRecomputeTaskCountsPatch.IsDisableTaskWin ||
				this.awakeCheckTaskGage <= ((float)compTask / (float)totalTask)
			)
		);
    }

    private static int computeRedPartNum(Type[] interfaces)
    {
        int num = 0;

        foreach (Type @interface in interfaces)
        {
            int addNum;
            string? name = @interface.FullName;
            name = name?.Replace("ExtremeRoles.Roles.API.Interface.","");
            switch (name)
            {
				case nameof(IRoleVoteModifier):
                    addNum = 9;
                    break;
                case nameof(IRoleMeetingButtonAbility):
                    addNum = 8;
                    break;
                case nameof(IRoleAwake<RoleTypes>):
                    addNum = 7;
                    break;
                case nameof(IRoleOnRevive):
                    addNum = 6;
                    break;
				case nameof(IRoleAbility):
				case nameof(IRoleUsableOverride):
					addNum = 5;
                    break;
                case nameof(IRoleMurderPlayerHook):
                    addNum = 4;
                    break;
                case nameof(IRoleUpdate):
                    addNum = 3;
                    break;
                case nameof(IRoleExilHook):
                case nameof(IRoleReportHook):
                    addNum = 2;
                    break;
                default:
                    addNum = 1;
                    break;
            }
            num += addNum;
        }

        return num;
    }

	private static IReadOnlyList<Vector3> getRedPartPos(in JToken json)
	{
		JArray? jsonRedPos = json.Get<JArray>("Red");

		if (jsonRedPos == null) { return []; }

		List<Vector3> redPos = new List<Vector3>(jsonRedPos.Count);
		for (int i = 0; i < jsonRedPos.Count; ++i)
		{
			JArray? pos = jsonRedPos.Get<JArray>(i);

			if (pos == null) { continue; }

			redPos.Add(
				new Vector3(
					(float)pos[0],
					(float)pos[1],
					(((float)pos[1]) / 1000.0f)));
		}
		return redPos;
	}
}

public sealed class Doll :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleUpdate,
    IParentChainStatus,
    IRoleWinPlayerModifier
{
    public enum AbilityType : byte
    {
        Admin,
        Security,
        VitalsLabel,
    }

    public ExtremeAbilityButton Button { get; set; }

    public byte Parent => this.hypnotistPlayerId;

    private byte hypnotistPlayerId;
    private byte dollPlayerId;

    private AbilityType curAbilityType;
    private AbilityType nextUseAbilityType;

    private string accessModule = string.Empty;
    private string crakingModule = string.Empty;

	private Minigame? minigame;
	private readonly HashSet<AbilityType> canUseCrakingModule;
	private readonly Hypnotist hypnotist;
#pragma warning disable CS8618
	private TMPro.TextMeshPro chargeTime;
	private TMPro.TextMeshPro tellText;

	private Sprite adminSprite;
    private Sprite securitySprite;
	private Sprite vitalSprite;

	private bool prevKillState;

    public Doll(
        byte dollPlayerId,
        byte hypnotistPlayerId,
        Hypnotist parent) : base(
        ExtremeRoleId.Doll,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Doll.ToString(),
        Palette.ImpostorRed,
        false, false, false,
        false, false, false,
        false, false, false)
    {
        this.dollPlayerId = dollPlayerId;
        this.hypnotistPlayerId = hypnotistPlayerId;
        this.hypnotist = parent;
        this.FakeImposter = true;
        this.canUseCrakingModule = new HashSet<AbilityType>();
        this.prevKillState = false;

        this.SetControlId(parent.GameControlId);
    }
#pragma warning restore CS8618

	public void FeatMapModuleAccess(SystemConsoleType consoleType)
    {
        switch (consoleType)
        {
            case SystemConsoleType.AdminModule:
                this.CanUseAdmin = true;
                break;
            case SystemConsoleType.SecurityCamera:
                this.CanUseSecurity = true;
                break;
            case SystemConsoleType.VitalsLabel:
                this.CanUseVital = true;
                break;
            default:
                break;
        }
        if (PlayerControl.LocalPlayer.PlayerId == this.dollPlayerId)
        {
            string consoleName = Tr.GetString(consoleType.ToString());

            showText(
				Tr.GetString(
					"FeatAccess",
					consoleName));
            this.accessModule =
                this.accessModule == string.Empty ?
                consoleName : $"{this.accessModule}, {consoleName}";
        }
    }

    public void UnlockCrakingAbility(SystemConsoleType consoleType)
    {
        AbilityType addType;
        switch (consoleType)
        {
            case SystemConsoleType.AdminModule:
                addType = AbilityType.Admin;
                break;
            case SystemConsoleType.SecurityCamera:
                addType = AbilityType.Security;
                break;
            case SystemConsoleType.VitalsLabel:
                addType = AbilityType.VitalsLabel;
                break;
            default:
                return;
        }
        if (this.canUseCrakingModule.Count == 0)
        {
            this.nextUseAbilityType = addType;
        }
        this.canUseCrakingModule.Add(addType);

        if (PlayerControl.LocalPlayer.PlayerId == this.dollPlayerId)
        {
            string consoleName = Tr.GetString(consoleType.ToString());

            showText(
				Tr.GetString(
					"unlockCraking",
					consoleName));
            this.crakingModule =
                this.crakingModule == string.Empty ?
                consoleName : $"{this.crakingModule}, {consoleName}";
        }
        this.Button?.SetButtonShow(true);
    }

    public void RemoveParent(byte rolePlayerId)
    {
        this.hypnotist.RemoveDoll(rolePlayerId);
    }

    public void ModifiedWinPlayer(
        NetworkedPlayerInfo rolePlayerInfo,
        GameOverReason reason,
		in WinnerTempData winner)
    {
        switch (reason)
        {
            case GameOverReason.ImpostorsByVote:
            case GameOverReason.ImpostorsByKill:
            case GameOverReason.ImpostorsBySabotage:
            case GameOverReason.ImpostorDisconnect:
            case GameOverReason.HideAndSeek_ImpostorsByKills:
            case (GameOverReason)RoleGameOverReason.AssassinationMarin:
			case (GameOverReason)RoleGameOverReason.TeroristoTeroWithShip:
				winner.AddWithPlus(rolePlayerInfo);
				break;
            default:
                break;
        }
    }

    public void CreateAbility()
    {
        this.adminSprite = GameSystem.GetAdminButtonImage();
        this.securitySprite = GameSystem.GetSecurityImage();
        this.vitalSprite = GameSystem.GetVitalImage();

        this.Button = RoleAbilityFactory.CreateBatteryAbility(
            "traitorCracking",
            this.adminSprite,
            IsAbilityUse,
            UseAbility,
            CheckAbility,
            CleanUp);

        this.Button.Behavior.SetCoolTime(
            hypnotist.DollCrakingCoolTime);
		if (this.Button.Behavior is IActivatingBehavior activatingBehavior)
		{
			activatingBehavior.ActiveTime = hypnotist.DollCrakingActiveTime;
		}
    }

    public bool UseAbility()
    {
        switch (this.nextUseAbilityType)
        {
            case AbilityType.Admin:
                HudManager.Instance.ToggleMapVisible(
                    new MapOptions
                    {
                        Mode = MapOptions.Modes.CountOverlay,
                        AllowMovementWhileMapOpen = true,
                        ShowLivePlayerPosition = true,
                        IncludeDeadBodies = true,
                    });
                break;
            case AbilityType.Security:
                SystemConsole? watchConsole = Map.GetSecuritySystemConsole();
                if (watchConsole == null || Camera.main == null)
                {
                    return false;
                }
                this.minigame = MinigameSystem.Open(
                    watchConsole.MinigamePrefab);
                break;
            case AbilityType.VitalsLabel:
                VitalsMinigame? vital = MinigameSystem.Vital;
                if (vital == null || Camera.main == null)
                {
                    return false;
                }
                this.minigame = MinigameSystem.Open(vital);
                break;
            default:
                return false;
        }

        this.curAbilityType = this.nextUseAbilityType;

        updateAbility();
        updateButtonSprite();

        return true;
    }

    public bool CheckAbility()
    {
        switch (this.curAbilityType)
        {
            case AbilityType.Admin:
                return MapBehaviour.Instance.isActiveAndEnabled;
            case AbilityType.Security:
            case AbilityType.VitalsLabel:
                return Minigame.Instance != null;
            default:
                return false;
        }
    }

    public void CleanUp()
    {
        switch (this.curAbilityType)
        {
            case AbilityType.Admin:
                if (MapBehaviour.Instance)
                {
                    MapBehaviour.Instance.Close();
                }
                break;
            case AbilityType.Security:
            case AbilityType.VitalsLabel:
                if (this.minigame != null)
                {
                    this.minigame.Close();
                    this.minigame = null;
                }
                break;
            default:
                break;
        }
    }

    public bool IsAbilityUse()
    {
        if (this.canUseCrakingModule.Count == 0) { return false; }

        switch (this.nextUseAbilityType)
        {
            case AbilityType.Admin:
                return
                    IRoleAbility.IsCommonUse() &&
                    (
                        MapBehaviour.Instance == null ||
                        !MapBehaviour.Instance.isActiveAndEnabled
                    );
            case AbilityType.Security:
            case AbilityType.VitalsLabel:
                return IRoleAbility.IsCommonUse() && Minigame.Instance == null;
            default:
                return false;
        }
    }

    public void ResetOnMeetingStart()
    {
        if (this.chargeTime != null)
        {
            this.chargeTime.gameObject.SetActive(false);
        }
        if (this.minigame != null)
        {
            this.minigame.Close();
            this.minigame = null;
        }
        if (MapBehaviour.Instance)
        {
            MapBehaviour.Instance.Close();
        }
        if (this.tellText != null)
        {
            this.tellText.gameObject.SetActive(false);
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    { }

    public void Update(PlayerControl rolePlayer)
    {
        PlayerControl hypnotistPlayer = Player.GetPlayerControlById(this.hypnotistPlayerId);
        if (!rolePlayer.Data.IsDead &&
            (hypnotistPlayer == null || hypnotistPlayer.Data.IsDead))
        {
            Player.RpcUncheckMurderPlayer(
                rolePlayer.PlayerId,
                rolePlayer.PlayerId, 0);
        }

		if (this.Button == null) { return; }

        if (this.canUseCrakingModule.Count == 0)
        {
            this.Button.SetButtonShow(false);
        }

        if (MeetingHud.Instance == null &&
            this.prevKillState != this.CanKill)
        {
            showText(
                this.CanKill ?
                Tr.GetString("unlockKill") :
                Tr.GetString("lockKill"));
        }

        this.prevKillState = this.CanKill;

        if (this.chargeTime == null)
        {
            this.chargeTime = UnityEngine.Object.Instantiate(
                HudManager.Instance.KillButton.cooldownTimerText,
                Camera.main.transform, false);
            this.chargeTime.transform.localPosition = new Vector3(3.5f, 2.25f, -250.0f);
        }

        if (!this.Button.IsAbilityActive())
        {
            this.chargeTime.gameObject.SetActive(false);
            return;
        }

        this.chargeTime.text = Mathf.CeilToInt(this.Button.Timer).ToString();
        this.chargeTime.gameObject.SetActive(true);
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole,
        byte targetPlayerId)
    {

        if (targetPlayerId == this.hypnotistPlayerId)
        {
            return Palette.ImpostorRed;
        }
        return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
    }

    public override string GetFullDescription()
    {
        var hypno = Player.GetPlayerControlById(this.hypnotistPlayerId);
        string fullDesc = base.GetFullDescription();

        if (hypno == null ||
			hypno.Data == null)
		{
			return fullDesc;
		}

        return string.Format(
            fullDesc, hypno.Data.PlayerName);
    }

    public override bool IsSameTeam(SingleRoleBase targetRole)
    {
        if (targetRole.Id == this.Id)
        {
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                return true;
            }
            else
            {
                return this.IsSameControlId(targetRole);
            }
        }
        else
        {
            return targetRole.IsImpostor();
        }
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        throw new Exception("Don't call this class method!!");
    }

    protected override void RoleSpecificInit()
    {
        throw new Exception("Don't call this class method!!");
    }

    private void updateAbility()
    {
        do
        {
            ++this.nextUseAbilityType;
            this.nextUseAbilityType = (AbilityType)((int)this.nextUseAbilityType % 3);
        }
        while (!this.canUseCrakingModule.Contains(this.nextUseAbilityType));
    }
    private void updateButtonSprite()
    {
        Sprite sprite = UnityObjectLoader.LoadSpriteFromResources(
            ObjectPath.TestButton);

        switch (this.nextUseAbilityType)
        {
            case AbilityType.Admin:
                sprite = this.adminSprite;
                break;
            case AbilityType.Security:
                sprite = this.securitySprite;
                break;
            case AbilityType.VitalsLabel:
                sprite = this.vitalSprite;
                break;
            default:
                break;
        }

        this.Button.Behavior.SetButtonImage(sprite);
    }

    private void showText(string text)
    {
        PlayerControl.LocalPlayer.StartCoroutine(coShowText(text));
    }

    private IEnumerator coShowText(string text)
    {
        if (this.tellText == null)
        {
            this.tellText = UnityEngine.Object.Instantiate(
                HudManager.Instance.TaskPanel.taskText,
                Camera.main.transform, false);
            this.tellText.transform.localPosition = new Vector3(0.0f, -0.9f, -250.0f);
            this.tellText.alignment = TMPro.TextAlignmentOptions.Center;
            this.tellText.gameObject.layer = 5;
        }
        this.tellText.text = text;
        this.tellText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3.5f);

        this.tellText.gameObject.SetActive(false);
    }
}

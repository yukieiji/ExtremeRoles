using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class CurseMaker :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleMurderPlayerHook,
    IRoleUpdate
{
    public enum CurseMakerOption
    {
        CursingRange,
        AdditionalKillCool,
        TaskCurseTimeReduceRate,
        IsNotRemoveDeadBodyByTask,
        NotRemoveDeadBodyTaskGage,
        IsDeadBodySearch,
        IsMultiDeadBodySearch,
        SearchDeadBodyTime,
        IsReduceSearchForTask,
        ReduceSearchTaskGage,
        ReduceSearchDeadBodyTime,
    }

    public class DeadBodyInfo
    {
        private DateTime killedTime;

        private byte killerPlayerId;
        private byte targetPlayerId;

        public DeadBodyInfo(
            PlayerControl killer,
            PlayerControl target)
        {
            this.killerPlayerId = killer.PlayerId;
            this.targetPlayerId = target.PlayerId;
            this.killedTime = DateTime.UtcNow;
        }

        public float ComputeDeltaTime()
        {
            TimeSpan deltaTime = DateTime.UtcNow - this.killedTime;
            return (float)deltaTime.TotalSeconds;
        }

        public DeadBody GetDeadBody()
        {
            DeadBody[] array = getAllDeadBody();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(
                        array[i].ParentId).PlayerId == this.targetPlayerId)
                {
                    return array[i];
                }
            }

            return null;
        }

        public byte GetKiller() => this.killerPlayerId;

        public byte GetTarget() => this.targetPlayerId;

        public bool IsValid()
        {
            DeadBody[] array = getAllDeadBody();
            for (int i = 0; i < array.Length; ++i)
            {
                if (GameData.Instance.GetPlayerById(
                        array[i].ParentId).PlayerId == this.targetPlayerId)
                {
                    return true;
                }
            }

            return false;
        }

        private DeadBody[] getAllDeadBody() => UnityEngine.Object.FindObjectsOfType<
            DeadBody>();


    }

    private Dictionary<byte, Arrow> deadBodyArrow;
    private Dictionary<byte, DeadBodyInfo> deadBodyData;

    private NetworkedPlayerInfo targetBody;
    private byte deadBodyId;

    private bool isDeadBodySearch = false;
    private bool isMultiDeadBodySearch = false;
    private bool isDeadBodySearchUsed = false;

    private float additionalKillCool = 1.0f;
    private float searchDeadBodyTime = 1.0f;
    private float deadBodyCheckRange = 1.0f;

    private string defaultButtonText;
    private string cursingText;

    private float prevTaskGage;

    private bool isReduceSearchByTask;
    private bool isReducedSearchTime;
    private float reduceSearchtaskGage;
    private float reduceTime;

    private float curCurseTime;
    private float curseTimeReduceRate;

    private bool isNotRemoveDeadBodyByTask;
    private bool isRemoveDeadBody = true;
    private float notRemoveDeadBodyTaskGage;

    public ExtremeAbilityButton Button
    {
        get => this.curseButton;
        set
        {
            this.curseButton = value;
        }
    }
    private ExtremeAbilityButton curseButton;

    public CurseMaker() : base(
        RoleCore.BuildCrewmate(
			ExtremeRoleId.CurseMaker,
			ColorPalette.CurseMakerViolet),
        false, true, false, false)
    { }

    public static void CurseKillCool(
        byte rolePlayerId, byte targetPlayerId)
    {

        if (PlayerControl.LocalPlayer.PlayerId != targetPlayerId) { return; }

        PlayerControl player = Player.GetPlayerControlById(targetPlayerId);

        if (player == null) { return; }
        if (player.Data.IsDead || player.Data.Disconnected) { return; }

        var curseMaker = ExtremeRoleManager.GetSafeCastedRole<CurseMaker>(
            rolePlayerId);
        if (curseMaker == null) { return; }

        var role = ExtremeRoleManager.GameRole[targetPlayerId];
        role.HasOtherKillCool = true;

        var multiAssignRole = role as MultiAssignRoleBase;
        if (multiAssignRole != null)
        {
            if (multiAssignRole.AnotherRole != null)
            {
                multiAssignRole.AnotherRole.HasOtherKillCool = true;
            }
        }

        RoleState.AddKillCoolOffset(curseMaker.additionalKillCool);

		player.killTimer =
			role.TryGetKillCool(out float resetKillCool) ?
			resetKillCool : Player.DefaultKillCoolTime;

        Sound.PlaySound(
            Sound.Type.CurseMakerCurse, 1.2f);
    }

    public void CreateAbility()
    {
        this.defaultButtonText = Tr.GetString("curse");

        this.CreateActivatingAbilityCountButton(
            "curse",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.CurseMakerCurse),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.deadBodyCheckRange);
        return IRoleAbility.IsCommonUse() && this.targetBody != null;
    }

    public void CleanUp()
    {

        PlayerControl rolePlayer = PlayerControl.LocalPlayer;

        if (this.isRemoveDeadBody)
        {
            Player.RpcCleanDeadBody(this.deadBodyId);
        }

        // 矢印消す
        if (this.deadBodyArrow.ContainsKey(this.deadBodyId))
        {
            deadBodyArrow[this.deadBodyId].Clear();
            deadBodyArrow.Remove(this.deadBodyId);
        }

        // 殺したやつを呪う
        DeadBodyInfo deadbodyInfo = this.deadBodyData[this.deadBodyId];
        byte killer = deadbodyInfo.GetKiller();
        byte target = deadbodyInfo.GetTarget();
        if (killer == target)
        {
            this.deadBodyId = byte.MaxValue;
            return;
        }

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CuresMakerCurseKillCool))
        {
            caller.WriteByte(rolePlayer.PlayerId);
            caller.WriteByte(killer);
        }
        CurseKillCool(rolePlayer.PlayerId, killer);
        this.deadBodyId = byte.MaxValue;

    }

    public bool CheckAbility()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.deadBodyCheckRange);

        bool result;

        if (this.targetBody == null)
        {
            result = false;
        }
        else
        {
            result = this.deadBodyId == this.targetBody.PlayerId;
        }

        this.Button.Behavior.SetButtonText(
            result ? this.cursingText : this.defaultButtonText);

        return result;
    }

    public bool UseAbility()
    {
        this.deadBodyId = this.targetBody.PlayerId;
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            CurseMakerOption.CursingRange,
            2.5f, 0.5f, 5.0f, 0.5f);

        factory.CreateFloatOption(
            CurseMakerOption.AdditionalKillCool,
            5.0f, 1.0f, 30.0f, 0.1f,
            format: OptionUnit.Second);

        IRoleAbility.CreateAbilityCountOption(
            factory, 1, 3, 5.0f);

        factory.CreateIntOption(
            CurseMakerOption.TaskCurseTimeReduceRate,
            0, 0, 10, 1,
            format: OptionUnit.Percentage);

        var removeDeadBodyOpt = factory.CreateBoolOption(
            CurseMakerOption.IsNotRemoveDeadBodyByTask,
            false);

        factory.CreateIntOption(
            CurseMakerOption.NotRemoveDeadBodyTaskGage,
            100, 0, 100, 5, removeDeadBodyOpt,
            format: OptionUnit.Percentage);

        var searchDeadBodyOption = factory.CreateBoolOption(
            CurseMakerOption.IsDeadBodySearch,
            true);

        factory.CreateBoolOption(
            CurseMakerOption.IsMultiDeadBodySearch,
            false, searchDeadBodyOption,
            invert: true);

        var searchTimeOpt = factory.CreateFloatOption(
            CurseMakerOption.SearchDeadBodyTime,
            60.0f, 0.5f, 120.0f, 0.5f,
            searchDeadBodyOption, format: OptionUnit.Second,
            invert: true);

        var taskBoostOpt = factory.CreateBoolOption(
            CurseMakerOption.IsReduceSearchForTask,
            false, searchDeadBodyOption,
            invert: true);

        factory.CreateIntOption(
            CurseMakerOption.ReduceSearchTaskGage,
            100, 25, 100, 5,
            taskBoostOpt,
            format: OptionUnit.Percentage,
            invert: true);

        var reduceTimeOpt = factory.CreateFloatDynamicOption(
            CurseMakerOption.ReduceSearchDeadBodyTime,
            30f, 0.5f, 0.5f, taskBoostOpt,
            format: OptionUnit.Second,
            invert: true,
            tempMaxValue: 120.0f);

        searchTimeOpt.AddWithUpdate(reduceTimeOpt);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;

        this.additionalKillCool = loader.GetValue<CurseMakerOption, float>(
            CurseMakerOption.AdditionalKillCool);
        this.deadBodyCheckRange = loader.GetValue<CurseMakerOption, float>(
            CurseMakerOption.CursingRange);
        this.curseTimeReduceRate = 1.0f - (loader.GetValue<CurseMakerOption, int>(
            CurseMakerOption.TaskCurseTimeReduceRate) / 100.0f);
        this.isNotRemoveDeadBodyByTask = loader.GetValue<CurseMakerOption, bool>(
            CurseMakerOption.IsNotRemoveDeadBodyByTask);
        this.notRemoveDeadBodyTaskGage = loader.GetValue<CurseMakerOption, int>(
            CurseMakerOption.NotRemoveDeadBodyTaskGage) / 100.0f;
        this.isDeadBodySearch = loader.GetValue<CurseMakerOption, bool>(
            CurseMakerOption.IsDeadBodySearch);
        this.isMultiDeadBodySearch = loader.GetValue<CurseMakerOption, bool>(
            CurseMakerOption.IsMultiDeadBodySearch);
        this.searchDeadBodyTime = loader.GetValue<CurseMakerOption, float>(
            CurseMakerOption.SearchDeadBodyTime);

        this.isDeadBodySearchUsed = false;
        this.isReduceSearchByTask = loader.GetValue<CurseMakerOption, bool>(
            CurseMakerOption.IsReduceSearchForTask);
        this.reduceSearchtaskGage = loader.GetValue<CurseMakerOption, int>(
            CurseMakerOption.ReduceSearchTaskGage) / 100.0f;
        this.reduceTime = loader.GetValue<CurseMakerOption, float>(
            CurseMakerOption.ReduceSearchDeadBodyTime);

        this.cursingText = Tr.GetString("cursing");
        this.deadBodyData = new Dictionary<byte, DeadBodyInfo>();
        this.deadBodyArrow = new Dictionary<byte, Arrow>();

        this.isRemoveDeadBody =
            this.isNotRemoveDeadBodyByTask && this.notRemoveDeadBodyTaskGage == 0.0f ? false : true;

        this.curCurseTime = loader.GetValue<RoleAbilityCommonOption, float>(
            RoleAbilityCommonOption.AbilityActiveTime);
    }

    public void ResetOnMeetingStart()
    {
        foreach (var arrow in deadBodyArrow.Values)
        {
            arrow.Clear();
        }

        deadBodyArrow.Clear();
        deadBodyData.Clear();
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void HookMuderPlayer(PlayerControl source, PlayerControl target)
    {
        if (MeetingHud.Instance != null || 
			this.isDeadBodySearchUsed || 
			!this.isDeadBodySearch)
		{
			return;
		}

        this.deadBodyData.Add(
            target.PlayerId,
            new DeadBodyInfo(source, target));

    }

    public void Update(PlayerControl rolePlayer)
    {
        foreach (var (playerId, arrow) in deadBodyArrow)
        {
            if (this.deadBodyData.ContainsKey(playerId))
            {
                var deadBodyInfo = this.deadBodyData[playerId];

                if (deadBodyInfo.IsValid())
                {
                    arrow.UpdateTarget(
                        deadBodyInfo.GetDeadBody().transform.position);
                    arrow.Update();
                }

            }
        }

        float taskGage = Player.GetPlayerTaskGage(rolePlayer);
        if (taskGage > this.prevTaskGage &&
			this.Button.Behavior is IActivatingBehavior activatingBehavior)
        {
            this.curCurseTime = this.curCurseTime * this.curseTimeReduceRate;
			activatingBehavior.ActiveTime = this.curCurseTime;
        }

        if (this.isReduceSearchByTask && !this.isReducedSearchTime)
        {
            if (taskGage >= this.reduceSearchtaskGage)
            {
                this.isReducedSearchTime = true;
                this.searchDeadBodyTime = Mathf.Clamp(
                    this.searchDeadBodyTime - this.reduceTime, 0.01f, this.searchDeadBodyTime);
            }
        }

        if (this.isNotRemoveDeadBodyByTask && this.isRemoveDeadBody)
        {
            if (taskGage >= this.notRemoveDeadBodyTaskGage)
            {
                this.isRemoveDeadBody = false;
            }
        }

        this.prevTaskGage = taskGage;

        if (this.isDeadBodySearchUsed || !this.isDeadBodySearch) { return; }

        List<byte> removeData = new List<byte>();

        foreach (var (playerId, deadBodyInfo) in this.deadBodyData)
        {
            if (deadBodyInfo.IsValid())
            {
                if (deadBodyInfo.ComputeDeltaTime() > this.searchDeadBodyTime &&
                    !this.deadBodyArrow.ContainsKey(playerId))
                {
                    var arrow = new Arrow(this.Core.Color);
                    this.deadBodyArrow.Add(playerId, arrow);
                    if (!this.isMultiDeadBodySearch)
                    {
                        this.isDeadBodySearchUsed = true;
                    }

                }
            }
            else
            {
                removeData.Add(playerId);
            }
        }

        foreach (byte deadBodyInfo in removeData)
        {
            this.deadBodyData.Remove(deadBodyInfo);
        }

    }
}

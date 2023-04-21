using System;
using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class CurseMaker : 
    SingleRoleBase, 
    IRoleAbility, 
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

    private GameData.PlayerInfo targetBody;
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
        ExtremeRoleId.CurseMaker,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.CurseMaker.ToString(),
        ColorPalette.CurseMakerViolet,
        false, true, false, false)
    { }

    public static void CurseKillCool(
        byte rolePlayerId, byte targetPlayerId)
    {

        if (CachedPlayerControl.LocalPlayer.PlayerId != targetPlayerId) { return; }

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

        if (role.TryGetKillCool(out float resetKillCool))
        {
            player.killTimer = resetKillCool;
        }
        else
        {
            player.killTimer = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
        }
        Sound.PlaySound(
            Sound.SoundType.CurseMakerCurse, 1.2f);
    }

    public void CreateAbility()
    {
        this.defaultButtonText = Translation.GetString("curse");

        this.CreateAbilityCountButton(
            "curse",
            Loader.CreateSpriteFromResources(
                Path.CurseMakerCurse),
            checkAbility: CheckAbility,
            abilityOff: CleanUp,
            forceAbilityOff: () => { });
        this.Button.SetLabelToCrewmate();
    }

    public bool IsAbilityUse()
    {
        this.targetBody = Player.GetDeadBodyInfo(
            this.deadBodyCheckRange);
        return this.IsCommonUse() && this.targetBody != null;
    }

    public void CleanUp()
    {

        PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

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
        IOptionInfo parentOps)
    {
        CreateFloatOption(
            CurseMakerOption.CursingRange,
            2.5f, 0.5f, 5.0f, 0.5f,
            parentOps);

        CreateFloatOption(
            CurseMakerOption.AdditionalKillCool,
            5.0f, 1.0f, 30.0f, 0.1f,
            parentOps, format: OptionUnit.Second);

        this.CreateAbilityCountOption(
            parentOps, 1, 3, 5.0f);

        CreateIntOption(
            CurseMakerOption.TaskCurseTimeReduceRate,
            0, 0, 10, 1, parentOps,
            format: OptionUnit.Percentage);

        var removeDeadBodyOpt = CreateBoolOption(
            CurseMakerOption.IsNotRemoveDeadBodyByTask,
            false, parentOps);

        CreateIntOption(
            CurseMakerOption.NotRemoveDeadBodyTaskGage,
            100, 0, 100, 5, removeDeadBodyOpt,
            format: OptionUnit.Percentage);

        var searchDeadBodyOption = CreateBoolOption(
            CurseMakerOption.IsDeadBodySearch,
            true, parentOps);

        CreateBoolOption(
            CurseMakerOption.IsMultiDeadBodySearch,
            false, searchDeadBodyOption,
            invert: true,
            enableCheckOption: parentOps);

        var searchTimeOpt = CreateFloatOption(
            CurseMakerOption.SearchDeadBodyTime,
            60.0f, 0.5f, 120.0f, 0.5f,
            searchDeadBodyOption, format: OptionUnit.Second,
            invert: true,
            enableCheckOption: parentOps);

        var taskBoostOpt = CreateBoolOption(
            CurseMakerOption.IsReduceSearchForTask,
            false, searchDeadBodyOption,
            invert: true,
            enableCheckOption: parentOps);

        CreateIntOption(
            CurseMakerOption.ReduceSearchTaskGage,
            100, 25, 100, 5,
            taskBoostOpt,
            format: OptionUnit.Percentage,
            invert: true,
            enableCheckOption: taskBoostOpt);

        var reduceTimeOpt = CreateFloatDynamicOption(
            CurseMakerOption.ReduceSearchDeadBodyTime,
            30f, 0.5f, 0.5f, taskBoostOpt,
            format: OptionUnit.Second,
            invert: true,
            enableCheckOption: taskBoostOpt,
            tempMaxValue: 120.0f);

        searchTimeOpt.SetUpdateOption(reduceTimeOpt);
    }

    protected override void RoleSpecificInit()
    {
        this.RoleAbilityInit();

        var allOption = AllOptionHolder.Instance;

        this.additionalKillCool = allOption.GetValue<float>(
            GetRoleOptionId(CurseMakerOption.AdditionalKillCool));
        this.deadBodyCheckRange = allOption.GetValue<float>(
            GetRoleOptionId(CurseMakerOption.CursingRange));
        this.curseTimeReduceRate = 1.0f - (allOption.GetValue<int>(
            GetRoleOptionId(CurseMakerOption.TaskCurseTimeReduceRate)) / 100.0f);
        this.isNotRemoveDeadBodyByTask = allOption.GetValue<bool>(
            GetRoleOptionId(CurseMakerOption.IsNotRemoveDeadBodyByTask));
        this.notRemoveDeadBodyTaskGage = allOption.GetValue<int>(
            GetRoleOptionId(CurseMakerOption.NotRemoveDeadBodyTaskGage)) / 100.0f;
        this.isDeadBodySearch = allOption.GetValue<bool>(
            GetRoleOptionId(CurseMakerOption.IsDeadBodySearch));
        this.isMultiDeadBodySearch = allOption.GetValue<bool>(
            GetRoleOptionId(CurseMakerOption.IsMultiDeadBodySearch));
        this.searchDeadBodyTime = allOption.GetValue<float>(
            GetRoleOptionId(CurseMakerOption.SearchDeadBodyTime));

        this.isDeadBodySearchUsed = false;
        this.isReduceSearchByTask = allOption.GetValue<bool>(
            GetRoleOptionId(CurseMakerOption.IsReduceSearchForTask));
        this.reduceSearchtaskGage = allOption.GetValue<int>(
            GetRoleOptionId(CurseMakerOption.ReduceSearchTaskGage)) / 100.0f;
        this.reduceTime = allOption.GetValue<float>(
            GetRoleOptionId(CurseMakerOption.ReduceSearchDeadBodyTime));

        this.cursingText = Translation.GetString("cursing");
        this.deadBodyData = new Dictionary<byte, DeadBodyInfo>();
        this.deadBodyArrow = new Dictionary<byte, Arrow>();

        this.isRemoveDeadBody = 
            this.isNotRemoveDeadBodyByTask && this.notRemoveDeadBodyTaskGage == 0.0f ? false : true;

        this.curCurseTime = allOption.GetValue<float>(
            GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime));
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

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void HookMuderPlayer(PlayerControl source, PlayerControl target)
    {
        if (this.isDeadBodySearchUsed || !this.isDeadBodySearch) { return; }

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
        if (taskGage > this.prevTaskGage)
        {
            this.curCurseTime = this.curCurseTime * this.curseTimeReduceRate;
            this.curseButton.Behavior.SetActiveTime(this.curCurseTime);
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
                    var arrow = new Arrow(this.NameColor);
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

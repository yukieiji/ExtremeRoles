using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Extension.Ship;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SandWorm : SingleRoleBase, IRoleAbility
{
    public sealed class AssaultButtonAutoActivator : IButtonAutoActivator
    {
        public bool IsActive()
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            return
                (
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue ||
                    FastDestroyableSingleton<HudManager>.Instance.UseButton.isActiveAndEnabled ||
                    isVentIn()
                ) &&
                localPlayer.Data != null &&
                MeetingHud.Instance == null &&
                ExileController.Instance == null &&
                !localPlayer.Data.IsDead;
        }
    }

    public sealed class SandWormAbilityBehavior : AbilityBehaviorBase
    {
        private Func<bool> ability;
        private Func<bool> canUse;

        private AbilityState prevState = AbilityState.None;

        public SandWormAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability) : base(text, img)
        {
            this.ability = ability;
            this.canUse = canUse;
        }

        public override void Initialize(ActionButton button)
        {
            return;
        }

        public override void AbilityOff()
        { }

        public override void ForceAbilityOff()
        { }

        public override bool IsCanAbilityActiving() => true;

        public override bool IsUse() => 
            this.canUse.Invoke();

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 || curState != AbilityState.Ready)
            {
                return false;
            }

            if (!this.ability.Invoke())
            {
                return false;
            }

            newState = AbilityState.CoolDown;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (!isVentIn() && !isLightOff())
            {
                if (curState != AbilityState.Stop)
                {
                    this.prevState = curState;
                }
                return AbilityState.Stop;
            }

            return curState == AbilityState.Stop ? this.prevState : curState;
        }
    }

    public enum SandWormOption
    {
        AssaultKillCoolReduce,
        KillCoolPenalty,
        AssaultRange,
    }

    public ExtremeAbilityButton Button
    {
        get => this.assaultButton;
        set
        {
            this.assaultButton = value;
        }
    }

    private float killPenalty;
    private float killBonus;

    private float range;

    private ExtremeAbilityButton assaultButton;
    private PlayerControl targetPlayer = null;

    public SandWorm() : base(
        ExtremeRoleId.SandWorm,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.SandWorm.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public override bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        if (isLightOff())
        {
            this.KillCoolTime = this.KillCoolTime - this.killBonus;
        }
        else
        {
            this.KillCoolTime = this.KillCoolTime + this.killPenalty;
        }

        this.KillCoolTime = Mathf.Clamp(this.KillCoolTime, 0.1f, float.MaxValue);
        
        return true;
    }


    public void CreateAbility()
    {
        this.Button = new ExtremeAbilityButton(
            new SandWormAbilityBehavior(
                Translation.GetString("assault"),
                FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite,
                IsAbilityUse, UseAbility),
            new AssaultButtonAutoActivator(),
            KeyCode.F);

        this.RoleAbilityInit();
    }

    public bool IsAbilityUse()
    {
        this.targetPlayer = Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer,
            this, this.range);

        return isVentIn() && this.targetPlayer != null;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.targetPlayer = null;
    }

    public bool UseAbility()
    {

        float prevTime = PlayerControl.LocalPlayer.killTimer;
        Helper.Logging.Debug($"PrevKillCool:{prevTime}");

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.StartVentAnimation))
        {
            caller.WritePackedInt(Vent.currentVent.Id);
        }

        RPCOperator.StartVentAnimation(
            Vent.currentVent.Id);

        Player.RpcUncheckMurderPlayer(
            CachedPlayerControl.LocalPlayer.PlayerId,
            this.targetPlayer.PlayerId,
            byte.MinValue);

        this.KillCoolTime = this.KillCoolTime - this.killBonus;
        this.KillCoolTime = Mathf.Clamp(this.KillCoolTime, 0.1f, float.MaxValue);

        this.targetPlayer = null;
        CachedPlayerControl.LocalPlayer.PlayerControl.SetKillTimer(prevTime);

        return true;
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateFloatOption(
            SandWormOption.KillCoolPenalty,
            5.0f, 1.0f, 10.0f, 0.1f,
            parentOps, format: OptionUnit.Second);

        CreateFloatOption(
            SandWormOption.AssaultKillCoolReduce,
            3.0f, 1.0f, 5.0f, 0.1f,
            parentOps, format: OptionUnit.Second);

        CreateFloatOption(
            SandWormOption.AssaultRange,
            2.0f, 0.1f, 3.0f, 0.1f,
            parentOps);

        CreateFloatOption(
            RoleAbilityCommonOption.AbilityCoolTime,
            15.0f, 0.5f, 45.0f, 0.1f,
            parentOps, format: OptionUnit.Second);

    }

    protected override void RoleSpecificInit()
    {
        this.range = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(SandWormOption.AssaultRange));

        this.killPenalty = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(SandWormOption.KillCoolPenalty));
        this.killBonus = AllOptionHolder.Instance.GetValue<float>(
            GetRoleOptionId(SandWormOption.AssaultKillCoolReduce));

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(
                FloatOptionNames.KillCooldown);
        }

        this.RoleAbilityInit();
    }

    private static bool isVentIn()
    {
        bool result = CachedPlayerControl.LocalPlayer.PlayerControl.inVent;
        Vent vent = Vent.currentVent;

        if (!result || vent == null) { return false; }

        if (CachedShipStatus.Instance.IsCustomVent(vent.Id)) { return false; }

        return true;
    }

    private static bool isLightOff()
    {
        foreach (PlayerTask task in
            CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
        {
            if (task.TaskType == TaskTypes.FixLights)
            {
                return true;
            }
        }
        return false;
    }
}

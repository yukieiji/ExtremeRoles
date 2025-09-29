using System;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Extension.VentModule;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior;


using ExtremeRoles.Module.CustomOption.Factory;



namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class SandWorm : SingleRoleBase, IRoleAbility, ITryKillTo
{
    public sealed class AssaultButtonAutoActivator : IButtonAutoActivator
    {
        public bool IsActive()
        {
            PlayerControl localPlayer = PlayerControl.LocalPlayer;

            return
                (
                    localPlayer.IsKillTimerEnabled ||
                    localPlayer.ForceKillTimerContinue ||
                    HudManager.Instance.UseButton.isActiveAndEnabled ||
                    isVentIn()
                ) &&
                localPlayer.Data != null &&
                MeetingHud.Instance == null &&
                ExileController.Instance == null &&
                !localPlayer.Data.IsDead;
        }
    }

    public sealed class SandWormAbilityBehavior : BehaviorBase
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
		RoleCore.BuildImpostor(ExtremeRoleId.SandWorm),
        true, false, true, true)
    { }

    public bool TryRolePlayerKillTo(
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
                Tr.GetString("assault"),
                HudManager.Instance.KillButton.graphic.sprite,
                IsAbilityUse, UseAbility),
            new AssaultButtonAutoActivator(),
            KeyCode.F);
		((IRoleAbility)(this)).RoleAbilityInit();
	}

    public bool IsAbilityUse()
    {
        this.targetPlayer = Player.GetClosestPlayerInRange(
            PlayerControl.LocalPlayer,
            this, this.range);

        return isVentIn() && this.targetPlayer != null;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        this.targetPlayer = null;
    }

    public bool UseAbility()
    {
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
        float prevTime = localPlayer.killTimer;
        Helper.Logging.Debug($"PrevKillCool:{prevTime}");

		int ventId = Vent.currentVent.Id;
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.StartVentAnimation))
        {
            caller.WritePackedInt(ventId);
        }
        RPCOperator.StartVentAnimation(ventId);

		byte targetPlayerId = this.targetPlayer.PlayerId;
		byte killerId = localPlayer.PlayerId;

		if (!Crewmate.BodyGuard.TryRpcKillGuardedBodyGuard(killerId, targetPlayerId))
		{
			Player.RpcUncheckMurderPlayer(
				killerId, targetPlayerId, byte.MinValue);
		}

        this.KillCoolTime = this.KillCoolTime - this.killBonus;
        this.KillCoolTime = Mathf.Clamp(this.KillCoolTime, 0.1f, float.MaxValue);

        this.targetPlayer = null;
		localPlayer.SetKillTimer(prevTime);

        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateFloatOption(
            SandWormOption.KillCoolPenalty,
            5.0f, 1.0f, 10.0f, 0.1f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            SandWormOption.AssaultKillCoolReduce,
            3.0f, 1.0f, 5.0f, 0.1f,
            format: OptionUnit.Second);

        factory.CreateFloatOption(
            SandWormOption.AssaultRange,
            2.0f, 0.1f, 3.0f, 0.1f);

        factory.CreateFloatOption(
            RoleAbilityCommonOption.AbilityCoolTime,
            15.0f, 0.5f, 45.0f, 0.1f,
            format: OptionUnit.Second);

    }

    protected override void RoleSpecificInit()
    {
		var cate = this.Loader;
        this.range = cate.GetValue<SandWormOption, float>(
            SandWormOption.AssaultRange);

        this.killPenalty = cate.GetValue<SandWormOption, float>(
            SandWormOption.KillCoolPenalty);
        this.killBonus = cate.GetValue<SandWormOption, float>(
            SandWormOption.AssaultKillCoolReduce);

        if (!this.HasOtherKillCool)
        {
            this.HasOtherKillCool = true;
            this.KillCoolTime = Player.DefaultKillCoolTime;
        }
    }

    private static bool isVentIn()
    {
        bool result = PlayerControl.LocalPlayer.inVent;
        Vent vent = Vent.currentVent;

        if (!result || vent.IsModed()) { return false; }

        return true;
    }

    private static bool isLightOff()
    {
        foreach (PlayerTask task in
            PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
        {
            if (task.TaskType == TaskTypes.FixLights)
            {
                return true;
            }
        }
        return false;
    }
}

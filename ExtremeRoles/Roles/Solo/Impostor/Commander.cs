using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Commander : SingleRoleBase, IRoleAutoBuildAbility, ITryKillTo
{
    public ExtremeAbilityButton Button
    {
        get => this.commandAttackButton;
        set
        {
            this.commandAttackButton = value;
        }
    }

    public enum CommanderOption
    {
        KillCoolReduceTime,
        KillCoolReduceImpBonus,
        IncreaseKillNum
    }

    private ExtremeAbilityButton commandAttackButton;
    private float killCoolReduceTime;
    private float killCoolImpNumBonus;
    private int increaseKillNum;
    private int killCount;

    public Commander() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Commander),
        true, false, true, true)
    { }

    public static void AttackCommad(byte rolePlayerId)
    {
        var role = ExtremeRoleManager.GetLocalPlayerRole();

        if (role == null || !role.IsImpostor()) { return; }

        Commander commander = ExtremeRoleManager.GetSafeCastedRole<Commander>(rolePlayerId);
        int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
            Int32OptionNames.NumImpostors);
        int deadImpNum = maxImpNum;
        foreach (var (playerId, checkRole) in ExtremeRoleManager.GameRole)
        {
            if (!checkRole.IsImpostor()) { continue; }

            var player = GameData.Instance.GetPlayerById(playerId);

            if (player == null || player.IsDead || player.Disconnected) { continue; }

            --deadImpNum;
        }

        deadImpNum = Mathf.Clamp(deadImpNum, 0, maxImpNum);

        float killCool = PlayerControl.LocalPlayer.killTimer;
        if (killCool > 0.1f)
        {
            float newKillCool = killCool -
                commander.killCoolReduceTime -
                (commander.killCoolImpNumBonus * deadImpNum);

            PlayerControl.LocalPlayer.killTimer = Mathf.Clamp(
                newKillCool, 0.1f, killCool);
        }
        Sound.PlaySound(
            Sound.Type.CommanderReduceKillCool, 1.2f);
    }
    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
           "attackCommand",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
			   ObjectPath.CommanderAttackCommand));
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        PlayerControl player = PlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.CommanderAttackCommand))
        {
            caller.WriteByte(player.PlayerId);
        }
        AttackCommad(player.PlayerId);

        return true;
    }

    public bool TryRolePlayerKillTo(
        PlayerControl rolePlayer, PlayerControl targetPlayer)
    {
        ++this.killCount;
        this.killCount %= this.increaseKillNum;
        if (this.killCount == 0 &&
            this.Button.Behavior is ICountBehavior countBehavior)
        {
            countBehavior.SetAbilityCount(countBehavior.AbilityCount + 1);
        }
        return true;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateNewFloatOption(
            CommanderOption.KillCoolReduceTime,
            2.0f, 0.5f, 5.0f, 0.1f,
            format: OptionUnit.Second);
        factory.CreateNewFloatOption(
            CommanderOption.KillCoolReduceImpBonus,
            1.5f, 0.1f, 3.0f, 0.1f,
            format: OptionUnit.Second);
        factory.CreateNewIntOption(
            CommanderOption.IncreaseKillNum,
            2, 1, 3, 1,
            format: OptionUnit.Shot);
        IRoleAbility.CreateAbilityCountOption(factory, 1, 3);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;
        this.killCoolReduceTime = cate.GetValue<CommanderOption, float>(
            CommanderOption.KillCoolReduceTime);
        this.killCoolImpNumBonus = cate.GetValue<CommanderOption, float>(
            CommanderOption.KillCoolReduceImpBonus);
        this.increaseKillNum = cate.GetValue<CommanderOption, int>(
            CommanderOption.IncreaseKillNum);

        this.killCount = 0;
    }
}

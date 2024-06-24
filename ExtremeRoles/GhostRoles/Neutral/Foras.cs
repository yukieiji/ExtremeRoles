using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;



using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;

namespace ExtremeRoles.GhostRoles.Neutal;

#nullable enable

public sealed class Foras : GhostRoleBase
{
    private ArrowControler? arrowControler;
    private PlayerControl? targetPlayer;

    private float range;
    private float delayTime;
    private int rate;

    public enum ForasOption
    {
        Range,
        DelayTime,
        MissingTargetRate,
    }

    public Foras() : base(
        false,
        ExtremeRoleType.Neutral,
        ExtremeGhostRoleId.Foras,
        ExtremeGhostRoleId.Foras.ToString(),
        ColorPalette.ForasSeeSyuTin)
    { }

    public static void SwitchArrow(ref MessageReader reader)
    {
        bool isShow = reader.ReadBoolean();
        byte forasPlayerId = reader.ReadByte();

        if (isShow)
        {
            showArrow(forasPlayerId, reader.ReadByte());
        }
        else
        {
            hideArrow(forasPlayerId);
        }
    }

    private static void showArrow(byte forasPlayerId, byte arrowTargetPlayerId)
    {
        var forasPlayer = Helper.Player.GetPlayerControlById(forasPlayerId);
        var arrowTargetPlayer = Helper.Player.GetPlayerControlById(arrowTargetPlayerId);

        if (!forasPlayer || !arrowTargetPlayer) { return; }
        Foras foras = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Foras>(forasPlayerId);
        var (role, anotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleHasParent>(
            forasPlayerId);

        if (foras is null || (role is null && anotherRole is null)) { return; }

        byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

        if (localPlayerId == forasPlayerId ||
            localPlayerId == role?.Parent ||
            localPlayerId == anotherRole?.Parent)
        {
            if (foras.arrowControler == null)
            {
                GameObject obj = new GameObject("Foras Arrow");
                foras.arrowControler = obj.AddComponent<ArrowControler>();
                foras.arrowControler.SetColor(foras.Color);
            }
            foras.arrowControler.SetTarget(arrowTargetPlayer.gameObject);
            foras.arrowControler.SetDelayActiveTimer(foras.delayTime);
            foras.arrowControler.SetHideTimer(
			   foras.Loader.GetValue<RoleAbilityCommonOption, float>(
                    RoleAbilityCommonOption.AbilityActiveTime));
            foras.arrowControler.gameObject.SetActive(true);
        }
    }
    private static void hideArrow(byte forasPlayerId)
    {
        Foras foras = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Foras>(forasPlayerId);
        if (foras.arrowControler != null)
        {
            foras.arrowControler.Hide();
        }
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.ForasShowArrow,
            Resources.Loader.CreateSpriteFromResources(
                Resources.Path.ForasShowArrow),
            this.isReportAbility(),
            () => true,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall, true,
            null, cleanUp);
        this.ButtonInit();
        this.Button.Behavior.SetActiveTime(
            this.Button.Behavior.ActiveTime + this.delayTime);
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.Sidekick,
        ExtremeRoleId.Servant
    };

    public override void Initialize()
    {
		var loader = this.Loader;
        this.delayTime = loader.GetValue<ForasOption, float>(ForasOption.DelayTime);
        this.range = loader.GetValue<ForasOption, float>(ForasOption.Range);
        this.rate = loader.GetValue<ForasOption, int>(ForasOption.MissingTargetRate);
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {

    }

    protected override void CreateSpecificOption(OptionFactory factory)
    {
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10, 25.0f);
		factory.CreateFloatOption(
            ForasOption.Range,
            1.0f, 0.1f, 3.6f, 0.1f);
		factory.CreateFloatOption(
            ForasOption.DelayTime,
            3.0f, 0.0f, 10.0f, 0.5f,
            format: OptionUnit.Second);
		factory.CreateIntOption(
            ForasOption.MissingTargetRate,
            10, 0, 90, 5,
            format: OptionUnit.Percentage);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        byte rolePlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

        if (this.rate > RandomGenerator.Instance.Next(101))
        {
            this.targetPlayer = CachedPlayerControl.AllPlayerControls
                .Where(x =>
                {
                    return
                        x != null &&
                        !x.Data.IsDead &&
                        !x.Data.Disconnected &&
                        x.PlayerId != rolePlayerId &&
                        x.PlayerId != this.targetPlayer!.PlayerId;
                })
                .OrderBy(x => RandomGenerator.Instance.Next())
                .First();
        }
        caller.WriteBoolean(true);
        caller.WriteByte(rolePlayerId);
        caller.WriteByte(this.targetPlayer!.PlayerId);
    }

    private bool isAbilityUse()
    {
        if (CachedShipStatus.Instance == null ||
            !CachedShipStatus.Instance.enabled) { return false; }


        this.targetPlayer = Helper.Player.GetClosestPlayerInRange(
            CachedPlayerControl.LocalPlayer,
            ExtremeRoleManager.GetLocalPlayerRole(),
            this.range);

        return IsCommonUse() && this.targetPlayer != null;
    }

    private void abilityCall()
    {
        showArrow(CachedPlayerControl.LocalPlayer.PlayerId, this.targetPlayer!.PlayerId);
        this.targetPlayer = null;
    }

    private void cleanUp()
    {
        PlayerControl player = CachedPlayerControl.LocalPlayer;

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UseGhostRoleAbility))
        {
            caller.WriteByte((byte)AbilityType.ForasShowArrow); // アビリティタイプ
            caller.WriteBoolean(false); // 報告できるかどうか
            caller.WriteBoolean(false);
            caller.WriteByte(player.PlayerId);
        }
        hideArrow(player.PlayerId);
    }
}

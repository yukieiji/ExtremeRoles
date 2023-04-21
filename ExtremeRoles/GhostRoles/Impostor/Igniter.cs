using System.Collections.Generic;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;


namespace ExtremeRoles.GhostRoles.Impostor;

public sealed class Igniter : GhostRoleBase
{
    public enum IgniterOption
    {
        IsEffectImpostor,
        IsEffectNeutral
    }

    private static bool isEffectImp;
    private static bool isEffectNeut;

    public Igniter() : base(
        false,
        ExtremeRoleType.Impostor,
        ExtremeGhostRoleId.Igniter,
        ExtremeGhostRoleId.Igniter.ToString(),
        Palette.ImpostorRed)
    { }

    public static bool TryComputeVison(GameData.PlayerInfo player, out float vison)
    {
        vison = float.MaxValue;
        SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];
        bool hasOtherVison = role.TryGetVisionMod(
            out float modVison, out bool isApplyVisonMod);
        float minVison = CachedShipStatus.Instance.MinLightRadius;
        
        if ((hasOtherVison && !isApplyVisonMod) || 
            (role.IsImpostor() && !isEffectImp) ||
            (role.IsNeutral() && !isEffectNeut))
        {
            return false;
        }
        else if (hasOtherVison)
        {
            vison = modVison * minVison;
            return true;
        }
        else if (role.IsImpostor())
        {
            vison = VisionComputer.ImpostorLightVision * minVison;
            return true;
        }
        else
        {
            vison = VisionComputer.CrewmateLightVision * minVison;
            return true;
        }
    }

    public static void SetVison(bool isLightOff)
    {
        var mod = isLightOff ?
            VisionComputer.Modifier.IgniterLightOff :
            VisionComputer.Modifier.None;

        VisionComputer.Instance.SetModifier(mod);
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.IgniterSwitchLight,
            Resources.Loader.CreateSpriteFromResources(
                Resources.Path.LastWolfLightOff),
            this.isReportAbility(),
            () => true,
            this.isAbilityUse,
            this.UseAbility,
            abilityCall, true,
            null, cleanUp);
        this.ButtonInit();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>()
    {
        ExtremeRoleId.LastWolf
    };

    public override void Initialize()
    {
        isEffectImp = AllOptionHolder.Instance.GetValue<bool>(
            GetRoleOptionId(IgniterOption.IsEffectImpostor));
        isEffectNeut = AllOptionHolder.Instance.GetValue<bool>(
            GetRoleOptionId(IgniterOption.IsEffectNeutral));
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {

    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {
        CreateCountButtonOption(
            parentOps, 3, 10, 15.0f);
        CreateBoolOption(
            IgniterOption.IsEffectImpostor,
            false, parent: parentOps);
        CreateBoolOption(
            IgniterOption.IsEffectNeutral,
            false, parent: parentOps);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        caller.WriteBoolean(true);
    }

    private bool isAbilityUse() => 
        this.IsCommonUse() &&
        VisionComputer.Instance.IsModifierResetted();
    
    private void abilityCall()
    {
        VisionComputer.Instance.SetModifier(
            VisionComputer.Modifier.IgniterLightOff);
    }

    private void cleanUp()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.UseGhostRoleAbility))
        {
            caller.WriteByte((byte)AbilityType.IgniterSwitchLight); // アビリティタイプ
            caller.WriteBoolean(false); // 報告できるかどうか
            caller.WriteBoolean(false);
        }
        SetVison(false);
    }
}

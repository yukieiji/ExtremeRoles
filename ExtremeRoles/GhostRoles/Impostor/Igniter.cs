using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using System.Collections.Generic;
using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.AutoParentSetOptionCategoryFactory;


#nullable enable

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

    public static bool TryComputeVison(NetworkedPlayerInfo player, out float vison)
    {
        vison = float.MaxValue;
        SingleRoleBase role = ExtremeRoleManager.GameRole[player.PlayerId];
        bool hasOtherVison = role.TryGetVisionMod(
            out float modVison, out bool isApplyVisonMod);
        float minVison = ShipStatus.Instance.MinLightRadius;

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
        this.Button = GhostRoleAbilityFactory.CreateActivatingCountAbility(
            AbilityType.IgniterSwitchLight,
            Resources.UnityObjectLoader.LoadSpriteFromResources(
                Resources.ObjectPath.LastWolfLightOff),
            this.IsReportAbility(),
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
		var loader = this.Loader;
        isEffectImp = loader.GetValue<IgniterOption, bool>(IgniterOption.IsEffectImpostor);
        isEffectNeut = loader.GetValue<IgniterOption, bool>(IgniterOption.IsEffectNeutral);
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {

    }

    protected override void CreateSpecificOption(AutoParentSetBuilder factory)
    {
		GhostRoleAbilityFactory.CreateCountButtonOption(factory, 3, 10, 15.0f);
		factory.CreateBoolOption(IgniterOption.IsEffectImpostor, false);
		factory.CreateBoolOption(IgniterOption.IsEffectNeutral, false);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        caller.WriteBoolean(true);
    }

    private bool isAbilityUse() =>
        IsCommonUse() &&
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

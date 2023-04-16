using System;
using System.Collections.Generic;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.GhostRoles.Neutal;

public sealed class Foras : GhostRoleBase
{
    private Arrow arrow;

    public enum ForasOption
    {
        IsEffectImpostor,
        IsEffectNeutral
    }

    public Foras() : base(
        false,
        ExtremeRoleType.Neutral,
        ExtremeGhostRoleId.Foras,
        ExtremeGhostRoleId.Foras.ToString(),
        Palette.ImpostorRed)
    { }

    public static void SwitchArrow(byte forasPlayerId, byte showTarget, bool show)
    {
        if (show)
        {
            showArrow(forasPlayerId, showTarget);
        }
        else
        {
            hideArrow(forasPlayerId);
        }
    }

    private static void showArrow(byte forasPlayerId, byte showTarget)
    {
        var forasPlayer = Helper.Player.GetPlayerControlById(forasPlayerId);
        var showTargetPlayer = Helper.Player.GetPlayerControlById(forasPlayerId);

        if (!forasPlayer || !showTargetPlayer) { return; }

        var (role, anotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleHasParent>(
            forasPlayerId);

        if (role is null && anotherRole is null) { return; }

        byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

        if (localPlayerId == forasPlayerId ||
            localPlayerId == role?.Parent ||
            localPlayerId == anotherRole?.Parent)
        {
            // showArrow logic
        }
    }
    private static void hideArrow(byte forasPlayerId)
    {
        Foras foras = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Foras>(forasPlayerId);
        foras.arrow?.SetActive(false);
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.ForasShowArrow,
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
        ExtremeRoleId.Sidekick,
        ExtremeRoleId.Servant
    };

    public override void Initialize()
    {
        
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {

    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        CreateCountButtonOption(
            parentOps, 3, 10, 25.0f);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {

    }

    private bool isAbilityUse() => this.IsCommonUse();

    private void abilityCall()
    {

    }

    private void cleanUp()
    {

    }
}

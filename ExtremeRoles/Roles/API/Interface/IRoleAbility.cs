using System;
using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.AbilityBehavior.Interface;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Roles.API.Interface;

public enum RoleAbilityCommonOption
{
    AbilityCoolTime = 35,
    AbilityCount,
    AbilityActiveTime,
}
public interface IRoleAbility : IRoleResetMeeting
{
    public ExtremeAbilityButton Button
    {
        get;
        set;
    }

    public void CreateAbility();

    public bool UseAbility();

    public bool IsAbilityUse();
}

public static class IRoleAbilityMixin
{
    private const float defaultCoolTime = 30.0f;
    private const float minCoolTime = 0.5f;
    private const float maxCoolTime = 120.0f;
    private const float minActiveTime = 0.5f;
    private const float maxActiveTime = 60.0f;
    private const float step = 0.5f;

    public static void CreateNormalAbilityButton(
        this IRoleAbility self,
        string textKey,
        Sprite sprite,
        Func<bool> checkAbility = null,
        Action abilityOff = null,
        Action forceAbilityOff = null,
        KeyCode hotkey = KeyCode.F)
    {

        self.Button = RoleAbilityFactory.CreateReusableAbility(
            textKey: textKey,
            img: sprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityOff,
            forceAbilityOff: forceAbilityOff,
            hotKey: hotkey);

        self.RoleAbilityInit();
    }

    public static void CreateAbilityCountButton(
        this IRoleAbility self,
        string textKey,
        Sprite sprite,
        Func<bool> checkAbility = null,
        Action abilityOff = null,
        Action forceAbilityOff = null,
        bool isReduceOnActive = false,
        KeyCode hotkey = KeyCode.F)
    {   
        self.Button = RoleAbilityFactory.CreateCountAbility(
            textKey: textKey,
            img: sprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityOff,
            forceAbilityOff: forceAbilityOff,
            isReduceOnActive: isReduceOnActive,
            hotKey: hotkey);

        self.RoleAbilityInit();

    }


    public static void CreateReclickableAbilityButton(
        this IRoleAbility self,
        string textKey,
        Sprite sprite,
        Func<bool> checkAbility = null,
        Action abilityOff = null,
        KeyCode hotkey = KeyCode.F)
    {
        self.Button = RoleAbilityFactory.CreateReclickAbility(
            textKey: textKey,
            img: sprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityOff,
            hotKey: hotkey);

        self.RoleAbilityInit();
    }

    public static void CreateReclickableCountAbilityButton(
        this IRoleAbility self,
        string textKey,
        Sprite sprite,
        Func<bool> checkAbility = null,
        Action abilityOff = null,
        KeyCode hotkey = KeyCode.F)
    {
        self.Button = RoleAbilityFactory.CreateReclickCountAbility(
            textKey: textKey,
            img: sprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityOff,
            hotKey: hotkey);

        self.RoleAbilityInit();
    }

    public static void CreateChargeAbilityButton(
        this IRoleAbility self,
        string textKey,
        Sprite sprite,
        Func<bool> checkAbility = null,
        Action abilityOff = null,
        Action forceAbilityOff = null,
        KeyCode hotkey = KeyCode.F)
    {

        self.Button = RoleAbilityFactory.CreateChargableAbility(
            textKey: textKey,
            img: sprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityOff,
            forceAbilityOff: forceAbilityOff,
            hotKey: hotkey);

        self.RoleAbilityInit();
    }

    public static void CreatePassiveAbilityButton(
        this IRoleAbility self,
        string activateButtonName,
        string deactivateButtonName,
        Sprite activateSprite,
        Sprite deactivateSprite,
        Action abilityCleanUp,
        Func<bool> checkAbility = null,
        KeyCode hotkey = KeyCode.F)
    {
        self.Button = RoleAbilityFactory.CreatePassiveAbility(
            activateTextKey: activateButtonName,
            activateImg: activateSprite,
            deactivateTextKey: deactivateButtonName,
            deactivateImg: deactivateSprite,
            canUse: self.IsAbilityUse,
            ability: self.UseAbility,
            canActivating: checkAbility,
            abilityOff: abilityCleanUp,
            hotKey: hotkey);

        self.RoleAbilityInit();
    }


    public static void CreateCommonAbilityOption(
        this IRoleAbility self,
        IOptionInfo parentOps,
        float defaultActiveTime = float.MaxValue)
    {

        SingleRoleBase role = (SingleRoleBase)self;

        new FloatCustomOption(
            self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime),
            string.Concat(
                role.RoleName,
                RoleAbilityCommonOption.AbilityCoolTime.ToString()),
            defaultCoolTime, minCoolTime, maxCoolTime, step,
            parentOps, format: OptionUnit.Second,
            tab: role.Tab);

        if (defaultActiveTime != float.MaxValue)
        {
            defaultActiveTime = Mathf.Clamp(
                defaultActiveTime, minActiveTime, maxActiveTime);

            new FloatCustomOption(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                string.Concat(
                    role.RoleName,
                    RoleAbilityCommonOption.AbilityActiveTime.ToString()),
                defaultActiveTime, minActiveTime, maxActiveTime, step,
                parentOps, format: OptionUnit.Second,
                tab: role.Tab);
        }

    }

    public static void CreateAbilityCountOption(
        this IRoleAbility self,
        IOptionInfo parentOps,
        int defaultAbilityCount,
        int maxAbilityCount,
        float defaultActiveTime = float.MaxValue)
    {

        SingleRoleBase role = (SingleRoleBase)self;

        self.CreateCommonAbilityOption(
            parentOps,
            defaultActiveTime);

        new IntCustomOption(
            self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCount),
            string.Concat(
                role.RoleName,
                RoleAbilityCommonOption.AbilityCount.ToString()),
            defaultAbilityCount, 1,
            maxAbilityCount, 1,
            parentOps, format: OptionUnit.Shot,
            tab: role.Tab);

    }

    public static int GetRoleOptionId(
        this IRoleAbility self,
        RoleAbilityCommonOption option) => ((RoleOptionBase)self).GetRoleOptionId((int)option);

    public static bool IsCommonUse(this IRoleAbility _)
    {
        return PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove;
    }

    public static void RoleAbilityInit(this IRoleAbility self)
    {

        if (self.Button == null) { return; }

        var allOpt = OptionManager.Instance;
        self.Button.Behavior.SetCoolTime(
            allOpt.GetValue<float>(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)));

        if (allOpt.TryGet<float>(
                self.GetRoleOptionId(RoleAbilityCommonOption.AbilityActiveTime),
                out var activeTimeOption))
        {
            self.Button.Behavior.SetActiveTime(activeTimeOption.GetValue());
        }

        if (self.Button.Behavior is ICountBehavior countBehavior)
        {
            countBehavior.SetAbilityCount(
                allOpt.GetValue<int>(self.GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)));
        }

        self.Button.OnMeetingEnd();
    }
}

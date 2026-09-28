using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class YoYo :
    SingleRoleBase,
    IRoleAutoBuildAbility,
    IRoleResetMeeting
{
    public sealed class YoYoAbilityBehavior :
        BehaviorBase, ICountBehavior
    {
        public int AbilityCount { get; private set; }
        public bool IsReduceAbilityCount { get; set; } = false;

        private Func<bool> ability;
        private Func<bool> canUse;
        private bool isUpdate = false;
        private TMPro.TextMeshPro? abilityCountText = null;
        private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

        public YoYoAbilityBehavior(
            string text, Sprite img,
            Func<bool> canUse,
            Func<bool> ability) : base(text, img)
        {
            this.ability = ability;
            this.canUse = canUse;
        }

        public void SetCountText(string text)
        {
            this.buttonTextFormat = text;
        }

        public override void Initialize(ActionButton button)
        {
            var coolTimerText = button.cooldownTimerText;

            this.abilityCountText = UnityEngine.Object.Instantiate(
                coolTimerText, coolTimerText.transform.parent);
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition +=
                new Vector3(-0.05f, 0.65f, 0);
            updateAbilityCountText();
        }

        public override void AbilityOff()
        { }

        public override void ForceAbilityOff()
        { }

        public override bool IsUse()
            => this.canUse.Invoke() && this.AbilityCount > 0;

        public override bool TryUseAbility(
            float timer, AbilityState curState, out AbilityState newState)
        {
            newState = curState;

            if (timer > 0 ||
                curState != AbilityState.Ready ||
                this.AbilityCount <= 0)
            {
                return false;
            }

            if (!this.ability.Invoke())
            {
                return false;
            }

            if (this.IsReduceAbilityCount)
            {
                this.reduceAbilityCount();
                this.IsReduceAbilityCount = false;
            }

            newState = AbilityState.CoolDown;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (this.isUpdate)
            {
                this.isUpdate = false;
                return AbilityState.CoolDown;
            }

            return
                this.AbilityCount > 0 ? curState : AbilityState.None;
        }

        public void SetAbilityCount(int newAbilityNum)
        {
            this.AbilityCount = newAbilityNum;
            this.isUpdate = true;
            updateAbilityCountText();
        }

        public void SetButtonTextFormat(string newTextFormat)
        {
            this.buttonTextFormat = newTextFormat;
            updateAbilityCountText();
        }

        private void reduceAbilityCount()
        {
            --this.AbilityCount;
            if (this.abilityCountText != null)
            {
                updateAbilityCountText();
            }
        }

        private void updateAbilityCountText()
        {
            if (this.abilityCountText != null)
            {
                this.abilityCountText.text = Tr.GetString(
                    this.buttonTextFormat,
                    this.AbilityCount);
            }
        }
    }

    public enum YoYoOption
    {
        IsResetMarkOnMeeting,
    }

    public ExtremeAbilityButton? Button { get; set; }

    public Vector2? SavedPosition { get; private set; }

    private YoYoAbilityBehavior? behavior;
    private bool isResetMarkOnMeeting;
    private GameObject? markerObject;

    public YoYo() : base(
        RoleArgs.BuildImpostor(ExtremeRoleId.YoYo))
    { }

    public void CreateAbility()
    {
        this.behavior = new YoYoAbilityBehavior(
            Tr.GetString("YoYoSaveLocation"),
            UnityObjectLoader.LoadFromResources(ExtremeRoleId.YoYo),
            IsAbilityUse,
            UseAbility);

        this.Button = new ExtremeAbilityButton(
            this.behavior,
            new RoleButtonActivator(),
            KeyCode.F);
        this.Button.SetLabelToCrewmate();

        this.RoleAbilityInit();
    }

    public void RoleAbilityInit()
    {
        if (this.Button == null || this.behavior == null)
        {
            return;
        }

        this.Button.Behavior.SetCoolTime(
            this.Loader.GetValue<RoleAbilityCommonOption, float>(
                RoleAbilityCommonOption.AbilityCoolTime));

        this.behavior.SetAbilityCount(
            this.Loader.GetValue<RoleAbilityCommonOption, int>(
                RoleAbilityCommonOption.AbilityCount));

        this.ClearMark();
        this.Button.OnMeetingEnd();
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public bool UseAbility() => UseAbilityInternal(true);

    public bool UseAbilityInternal(bool isCreateMarker)
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
        if (localPlayer == null)
        {
            return false;
        }

        if (this.behavior != null)
        {
            this.behavior.IsReduceAbilityCount = this.SavedPosition.HasValue;
        }

        if (this.SavedPosition.HasValue)
        {
            Vector2 targetPos = this.SavedPosition.Value;
            Helper.Player.RpcUncheckSnap(localPlayer.PlayerId, targetPos);
            this.ClearMark();
            return true;
        }
        else
        {
            Vector2 currentPos = localPlayer.GetTruePosition();
            this.SetMark(currentPos, isCreateMarker);
            return true;
        }
    }

    public void SetMark(Vector2 pos, bool isCreateMarker = true)
    {
        this.SavedPosition = pos;

        if (this.markerObject != null)
        {
            UnityEngine.Object.Destroy(this.markerObject);
            this.markerObject = null;
        }

        if (isCreateMarker)
        {
            this.markerObject = new GameObject("YoYoMark");
            this.markerObject.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

            SpriteRenderer renderer = this.markerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = UnityObjectLoader.LoadFromResources(ExtremeRoleId.YoYo);
            renderer.color = new Color(1.0f, 1.0f, 1.0f, 0.6f);
        }

        if (this.behavior != null)
        {
            string newText = Tr.GetString("YoYoTeleportLocation");
            this.behavior.SetButtonText(newText);
            this.behavior.SetGraphic(newText, UnityObjectLoader.LoadFromResources(ExtremeRoleId.YoYo));
        }
    }

    public void ClearMark()
    {
        this.SavedPosition = null;

        if (this.markerObject != null)
        {
            UnityEngine.Object.Destroy(this.markerObject);
            this.markerObject = null;
        }

        if (this.behavior != null)
        {
            string newText = Tr.GetString("YoYoSaveLocation");
            this.behavior.SetButtonText(newText);
            this.behavior.SetGraphic(newText, UnityObjectLoader.LoadFromResources(ExtremeRoleId.YoYo));
        }
    }

    public void ResetOnMeetingStart()
    {
        if (this.isResetMarkOnMeeting)
        {
            this.ClearMark();
        }
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(factory, 3, 10);
        factory.CreateBoolOption(
            YoYoOption.IsResetMarkOnMeeting,
            true);
    }

    protected override void RoleSpecificInit()
    {
        var loader = this.Loader;
        this.isResetMarkOnMeeting = loader.GetValue<YoYoOption, bool>(
            YoYoOption.IsResetMarkOnMeeting);
    }
}

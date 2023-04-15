using System;
using System.Linq;

using AmongUs.GameOptions;
using UnityEngine;

using Newtonsoft.Json.Linq;

using ExtremeRoles.Compat.Mods;
using ExtremeRoles.Extension.Json;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.AbilityBehavior.Interface;
using ExtremeRoles.Module.ButtonAutoActivator;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class Teleporter : 
    SingleRoleBase, IRoleAbility, IRoleSpecialSetUp
{
    public sealed class TeleporterAbilityBehavior : 
        AbilityBehaviorBase, ICountBehavior
    {
        public int AbilityCount { get; private set; }
        public bool IsReduceAbilityCount { get; set; } = false;

        private Func<bool> ability;
        private Func<bool> canUse;
        private bool isUpdate = false;
        private TMPro.TextMeshPro abilityCountText = null;
        private string buttonTextFormat = ICountBehavior.DefaultButtonCountText;

        public TeleporterAbilityBehavior(
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

        public override bool IsCanAbilityActiving() => true;

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

            newState = this.ActiveTime <= 0.0f ?
                AbilityState.CoolDown : AbilityState.Activating;

            return true;
        }

        public override AbilityState Update(AbilityState curState)
        {
            if (curState == AbilityState.Activating)
            {
                return curState;
            }

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
            this.abilityCountText.text = string.Format(
                Translation.GetString(this.buttonTextFormat),
                this.AbilityCount);
        }
    }

    public enum TeleporterOption
    {
        CanUseOtherPlayer
    }

    public ExtremeAbilityButton Button { get; set; }

    private bool isSharePortal;
    private int partNum;
    private TeleporterAbilityBehavior behavior;
    private PortalFirst portal;

    private Sprite firstPortalImg;
    private Sprite secondPortalImg;

    private const string postionJson =
        "ExtremeRoles.Resources.JsonData.TeleporterTeleportPartPosition.json";

    private const string skeldKey = "Skeld";
    private const string miraHqKey = "MiraHQ";
    private const string polusKey = "Polus";
    private const string airShipKey = "AirShip";

    public Teleporter() : base(
        ExtremeRoleId.Teleporter,
        ExtremeRoleType.Crewmate,
        ExtremeRoleId.Teleporter.ToString(),
        ColorPalette.MaintainerBlue,
        false, true, false, false)
    { }

    public static void SetPortal(byte teleporterPlayerId, Vector2 pos)
    {
        Teleporter teleporter = ExtremeRoleManager.GetSafeCastedRole<Teleporter>(
            teleporterPlayerId);

        GameObject obj = new GameObject("portal");
        obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            ExtremeRolesPlugin.Compat.ModMap.AddCustomComponent(
                obj, 
                Compat.Interface.CustomMonoBehaviourType.MovableFloorBehaviour);
        }

        if (teleporter.portal == null)
        {
            teleporter.portal = obj.AddComponent<PortalFirst>();
        }
        else
        {
            PortalSecond potal = obj.AddComponent<PortalSecond>();
            PortalBase.Link(potal, teleporter.portal);
            teleporter.portal = null;
        }
    }

    public void IncreaseAbilityCount()
    {
        this.behavior.SetAbilityCount(
            this.behavior.AbilityCount + 1);
    }

    public void IntroBeginSetUp()
    {
        return;
    }

    public void IntroEndSetUp()
    {
        string key = skeldKey;

        if (ExtremeRolesPlugin.Compat.IsModMap)
        {

            if (ExtremeRolesPlugin.Compat.ModMap is SubmergedMap)
            {
                key = "Submerged";
            }
        }
        else
        {
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId);
            key = mapId switch
            {
                0 => skeldKey,
                1 => miraHqKey,
                2 => polusKey,
                4 => airShipKey,
                _ => skeldKey,
            };
        }
        var position = JsonParser.GetJObjectFromAssembly(postionJson);
        setPartFromMapJsonInfo(position.Get<JArray>(key), this.partNum);
    }

    public void CreateAbility()
    {
        this.firstPortalImg = Loader.CreateSpriteFromResources(
            Path.TeleporterFirstPortal);
        this.secondPortalImg = Loader.CreateSpriteFromResources(
            Path.TeleporterSecondPortal);

        this.behavior = new TeleporterAbilityBehavior(
            Translation.GetString("SetPortal"),
            this.firstPortalImg,
            IsAbilityUse, UseAbility);

        this.Button = new ExtremeAbilityButton(
            this.behavior,
            new RoleButtonActivator(),
            KeyCode.F);
        this.Button.SetLabelToCrewmate();
        this.abilityInit();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
        byte playerId = localPlayer.PlayerId;
        Vector2 pos = localPlayer.GetTruePosition();

        this.behavior.IsReduceAbilityCount = this.portal != null;

        if (this.isSharePortal)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.TeleporterSetPortal))
            {
                caller.WriteByte(playerId);
                caller.WriteFloat(pos.x);
                caller.WriteFloat(pos.y);
            }
        }
        SetPortal(playerId, pos);

        this.behavior.SetButtonImage(
            this.behavior.IsReduceAbilityCount ?
            this.firstPortalImg : this.secondPortalImg);

        return true;
    }

    public bool IsAbilityUse() => this.IsCommonUse();

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        IOption parentOps)
    {
        CreateBoolOption(
            TeleporterOption.CanUseOtherPlayer,
            false, parentOps);
        this.CreateAbilityCountOption(
            parentOps, 1, 3);
    }

    protected override void RoleSpecificInit()
    {
        this.isSharePortal = OptionHolder.AllOption[
            GetRoleOptionId(TeleporterOption.CanUseOtherPlayer)].GetValue();
        this.partNum = OptionHolder.AllOption[
            GetRoleOptionId(RoleAbilityCommonOption.AbilityCount)].GetValue();
        this.abilityInit();
    }

    private void abilityInit()
    {
        if (this.Button == null) { return; }

        var allOpt = OptionHolder.AllOption;
        this.Button.Behavior.SetCoolTime(
            allOpt[this.GetRoleOptionId(
                RoleAbilityCommonOption.AbilityCoolTime)].GetValue());
        
        this.behavior.SetAbilityCount(0);

        this.Button.OnMeetingEnd();
    }

    private static void setPartFromMapJsonInfo(JArray json, int num)
    {
        int[] randomIndex = Enumerable.Range(0, json.Count).OrderBy(
            x => RandomGenerator.Instance.Next()).ToArray();
        for (int i = 0; i < num; ++i)
        {
            JArray pos = json[randomIndex[i]].TryCast<JArray>();
            GameObject obj = new GameObject("portalPart");
            obj.transform.position = new Vector3(
                (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
            obj.AddComponent<TeleporterPortalPart>();
        }
    }
}

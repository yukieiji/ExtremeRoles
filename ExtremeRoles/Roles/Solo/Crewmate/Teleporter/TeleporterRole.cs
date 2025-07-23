using System;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Compat;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Crewmate.Teleporter;

public sealed class TeleporterRole :
    SingleRoleBase, IRoleAutoBuildAbility, IRoleSpecialSetUp
{
    public sealed class TeleporterAbilityBehavior :
        BehaviorBase, ICountBehavior
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
            this.abilityCountText.text = Tr.GetString(
				this.buttonTextFormat,
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

    public Teleporter() : base(
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Teleporter,
			ColorPalette.TeleporterCherry),
        false, true, false, false)
    { }

    public static void SetPortal(byte teleporterPlayerId, Vector2 pos)
    {
        Teleporter teleporter = ExtremeRoleManager.GetSafeCastedRole<Teleporter>(
            teleporterPlayerId);

        GameObject obj = new GameObject("portal");
        obj.transform.position = new Vector3(pos.x, pos.y, pos.y / 1000.0f);

        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
			modMap!.AddCustomComponent(obj,
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

	public void RoleAbilityInit()
	{
		if (this.Button == null) { return; }

		this.Button.Behavior.SetCoolTime(
			this.Loader.GetValue<RoleAbilityCommonOption, float>(
				RoleAbilityCommonOption.AbilityCoolTime));

		this.behavior.SetAbilityCount(0);

		this.Button.OnMeetingEnd();
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
        var position = JsonParser.GetJObjectFromAssembly(postionJson);
        setPartFromMapJsonInfo(
			position.Get<JArray>(Map.Name),
			this.partNum);
    }

    public void CreateAbility()
    {
		this.firstPortalImg = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Teleporter, ObjectPath.TeleporterFirstPortal);
        this.secondPortalImg = UnityObjectLoader.LoadFromResources(
			ExtremeRoleId.Teleporter, ObjectPath.TeleporterSecondPortal);

		this.behavior = new TeleporterAbilityBehavior(
            Tr.GetString("SetPortal"),
            this.firstPortalImg,
            IsAbilityUse, UseAbility);

        this.Button = new ExtremeAbilityButton(
            this.behavior,
            new RoleButtonActivator(),
            KeyCode.F);
        this.Button.SetLabelToCrewmate();
		this.RoleAbilityInit();
    }

    public bool UseAbility()
    {
        PlayerControl localPlayer = PlayerControl.LocalPlayer;
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

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingStart()
    {
        return;
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    protected override void CreateSpecificOption(
        AutoParentSetOptionCategoryFactory factory)
    {
        factory.CreateBoolOption(
            TeleporterOption.CanUseOtherPlayer,
            false);
        IRoleAbility.CreateAbilityCountOption(
            factory, 1, 3);
    }

    protected override void RoleSpecificInit()
    {
		var loader = this.Loader;
        this.isSharePortal = loader.GetValue<TeleporterOption, bool>(
            TeleporterOption.CanUseOtherPlayer);
        this.partNum = loader.GetValue<RoleAbilityCommonOption, int>(
            RoleAbilityCommonOption.AbilityCount);
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

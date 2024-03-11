using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.AbilityModeSwitcher;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.ButtonAutoActivator;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factories.AutoParentSetFactory;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType;

#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class Kids : GhostAndAliveCombinationRoleManagerBase
{
    public const string Name = "Kids";

    public Kids() : base(
        Name, ColorPalette.KidsYellowGreen, 2,
        GameSystem.MaxImposterNum)
    {
        this.Roles.Add(new Delinquent());

        this.CombGhostRole.Add(
            ExtremeRoleId.Delinquent, new Wisp());
    }

    public override void InitializeGhostRole(
        byte rolePlayerId, GhostRoleBase role, SingleRoleBase aliveRole)
    {
        if (aliveRole is Delinquent delinquent &&
            role is Wisp wisp)
        {
            if (rolePlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                wisp.SetAbilityNum(delinquent.WispAbilityNum);
            }
            wisp.SetWinPlayerNum(rolePlayerId);
        }
    }
}

public sealed class Delinquent : MultiAssignRoleBase, IRoleAutoBuildAbility
{
	public enum AbilityType : byte
	{
		Scribe,
		SelfBomb,
	}

	public override bool IsAssignGhostRole => this.canAssignWisp;

    public sealed class DelinquentAbilityBehavior : AbilityBehaviorBase
    {
        public int AbilityCount { get; private set; }
        public AbilityType CurAbility { get; private set; }

        private bool isUpdate;

        private TextMeshPro? abilityCountText;
        private Func<bool> useAbility;
        private Func<bool> canUse;

        private GraphicSwitcher<AbilityType> switcher;

        public DelinquentAbilityBehavior(
            GraphicMode scribeMode,
            GraphicMode bombMode,
            Func<bool> canUse,
            Func<bool> useAbility) : base(
                scribeMode.Graphic.Text,
                scribeMode.Graphic.Img)
        {
            this.useAbility = useAbility;
            this.canUse = canUse;

            this.switcher = new GraphicSwitcher<AbilityType>(this);
            this.switcher.Add(AbilityType.Scribe, scribeMode);
            this.switcher.Add(AbilityType.SelfBomb, bombMode);
        }

        public void SetAbilityCount(int newAbilityNum)
        {
            this.AbilityCount = newAbilityNum;
            this.isUpdate = true;
            updateAbilityInfoText();
        }

        public override void AbilityOff()
        { }

        public override void ForceAbilityOff()
        { }

        public override void Initialize(ActionButton button)
        {
            var coolTimerText = button.cooldownTimerText;

            this.abilityCountText = UnityEngine.Object.Instantiate(
                coolTimerText, coolTimerText.transform.parent);
            this.abilityCountText.enableWordWrapping = false;
            this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
            this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            updateAbilityInfoText();
        }

        public override bool IsCanAbilityActiving() => true;

        public override bool IsUse() =>
            this.canUse.Invoke() && this.AbilityCount > 0;

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

            if (!this.useAbility.Invoke())
            {
                return false;
            }

            --this.AbilityCount;
            bool updateBomb = this.AbilityCount <= 0;
            if (updateBomb && this.CurAbility == AbilityType.Scribe)
            {
                this.AbilityCount = 1;
                this.CurAbility = AbilityType.SelfBomb;
            }
            if (this.abilityCountText != null)
            {
                if (!updateBomb)
                {
                    updateAbilityInfoText();
                }
                else
                {
                    this.abilityCountText.gameObject.SetActive(false);
                }
            }

            this.switcher.Switch(this.CurAbility);
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

            return this.AbilityCount > 0 ? curState : AbilityState.None;
        }

        private void updateAbilityInfoText()
        {
            this.abilityCountText!.text = string.Format(
                Translation.GetString("scribeText"), this.AbilityCount);
        }
    }

    public ExtremeAbilityButton? Button { get; set; }

    public bool WinCheckEnable => this.isWinCheck;
    public float Range => this.range;

    public enum DelinqentOption
    {
        Range,
    }

    public int WispAbilityNum => this.abilityCount;

    private AbilityType curAbilityType;
    private float range;
    private bool isWinCheck;

    private int abilityCount = 0;
    private const int maxImageNum = 10;

    private bool canAssignWisp = true;

    public Delinquent() : base(
        ExtremeRoleId.Delinquent,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Delinquent.ToString(),
        ColorPalette.KidsYellowGreen,
        false, false, false, false,
        tab: OptionTab.Combination)
    { }

    public static void Ability(ref MessageReader reader)
    {
		AbilityType abilityType = (AbilityType)reader.ReadByte();
		byte playerId = reader.ReadByte();

		Delinquent delinquent =
            ExtremeRoleManager.GetSafeCastedRole<Delinquent>(playerId);
        switch (abilityType)
        {
            case AbilityType.Scribe:
				PlayerControl rolePlayer = Player.GetPlayerControlById(playerId);
				setScibe(rolePlayer, delinquent);
                break;
            case AbilityType.SelfBomb:
                setBomb(delinquent);
                break;
        }
    }

    private static void setScibe(PlayerControl player, Delinquent delinquent)
    {
        GameObject obj = new GameObject("Scribe");
        obj.transform.position = player.transform.position;
        SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
        rend.sprite = Loader.CreateSpriteFromResources(
            string.Format(
                Path.DelinquentScribe,
                RandomGenerator.Instance.Next(0, maxImageNum)));
        delinquent.abilityCount++;
    }
    private static void setBomb(Delinquent delinquent)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            delinquent.isWinCheck = true;
        }
    }

    public void BlockWispAssign()
    {
        this.canAssignWisp = false;
    }

    public void CreateAbility()
    {
        this.Button = new ExtremeAbilityButton(
            new DelinquentAbilityBehavior(
                new GraphicMode()
                {
                    Graphic = new ButtonGraphic(
                        Translation.GetString("scribble"),
                        Loader.CreateSpriteFromResources(
                            string.Format(
                                Path.DelinquentScribe,
                                RandomGenerator.Instance.Next(0, maxImageNum))))
                },
                new GraphicMode()
                {
                    Graphic = new ButtonGraphic(
                        Translation.GetString("selfBomb"),
                        Loader.CreateSpriteFromResources(
                            Path.BomberSetBomb))
                },
                this.IsAbilityUse,
                this.UseAbility),
            new RoleButtonActivator(),
            KeyCode.F);

		((IRoleAbility)(this)).RoleAbilityInit();

		if (this.Button?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                OptionManager.Instance.GetValue<int>(GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)));
        }
    }

    public bool IsAbilityUse()
    {
        if (!(this.Button?.Behavior is DelinquentAbilityBehavior behavior))
        {
            return false;
        }

        this.curAbilityType = behavior.CurAbility;

        return this.curAbilityType switch
        {
            AbilityType.Scribe =>
                IRoleAbility.IsCommonUse(),
            AbilityType.SelfBomb =>
                Player.GetClosestPlayerInRange(
                    CachedPlayerControl.LocalPlayer, this, this.range) != null,
            _ => true
        };
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.KidsAbility))
        {
            caller.WriteByte((byte)this.curAbilityType);
            caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
        }
        switch (this.curAbilityType)
        {
            case AbilityType.Scribe:
                setScibe(CachedPlayerControl.LocalPlayer, this);
                break;
            case AbilityType.SelfBomb:
                setBomb(this);
                break;
            default:
                break;
        }
        return true;
    }

    protected override void CreateSpecificOption(IOptionInfo parentOps)
    {
        this.CreateAbilityCountOption(parentOps, 7, 20);
        CreateFloatOption(
            DelinqentOption.Range,
            3.6f, 1.0f, 5.0f, 0.1f,
            parentOps);
    }

    protected override void RoleSpecificInit()
    {
        this.curAbilityType = AbilityType.Scribe;

        this.abilityCount = 0;

        this.range = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(DelinqentOption.Range));

        if (this.Button?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                OptionManager.Instance.GetValue<int>(GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)));
        }

        this.canAssignWisp = true;
    }

}

public sealed class Wisp : GhostRoleBase, IGhostRoleWinable
{

    public enum WispOption
    {
        WinNum,
        TorchAbilityNum,
        TorchNum,
        TorchRange,
        TorchActiveTime,
        BlackOutTime,
    }

    private int abilityNum;
    private int winNum;
    private int torchNum;
    private float range;
    private float torchActiveTime;
    private float torchBlackOutTime;

	private WispTorchSystem? system;

    public Wisp() : base(
        false, ExtremeRoleType.Neutral,
        ExtremeGhostRoleId.Wisp,
        ExtremeGhostRoleId.Wisp.ToString(),
        ColorPalette.KidsYellowGreen,
        OptionTab.Combination)
    { }

    public void SetAbilityNum(int abilityNum)
    {
		if (this.Button?.Behavior is AbilityCountBehavior behavior)
		{
			behavior.SetAbilityCount(abilityNum + this.abilityNum);
		}
    }


    public bool IsWin(
        GameOverReason reason,
        GameData.PlayerInfo ghostRolePlayer) => this.system != null && this.system.IsWin(this);

    public void SetWinPlayerNum(byte rolePlayerId)
    {
        foreach (GameData.PlayerInfo player in
            GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (player == null ||
                player.IsDead ||
                player.Disconnected ||
                player.PlayerId == rolePlayerId) { continue; }

            this.winNum++;
        }
        // 負の値にならないようにする
        this.winNum = Math.Clamp(this.winNum, 1, int.MaxValue);

		if (this.system is null)
		{
			tryAddWispSystem();
		}
		this.system!.SetWinPlayerNum(this, this.winNum);
	}

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.winNum,
			this.system?.CurEffectPlayerNum(this));
    }

    public override void CreateAbility()
    {
        this.Button = GhostRoleAbilityFactory.CreateCountAbility(
            AbilityType.WispSetTorch,
            Loader.CreateSpriteFromResources(
                Path.WispTorch),
            this.isReportAbility(),
            () => true,
            () => true,
            this.UseAbility,
            () => { }, true);
        this.ButtonInit();
        this.Button.SetLabelToCrewmate();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    public override void Initialize()
    {
        this.abilityNum = OptionManager.Instance.GetValue<int>(
            GetRoleOptionId(WispOption.TorchAbilityNum));
        this.winNum = OptionManager.Instance.GetValue<int>(
            GetRoleOptionId(WispOption.WinNum));
        this.torchNum = OptionManager.Instance.GetValue<int>(
            GetRoleOptionId(WispOption.TorchNum));
        this.range = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(WispOption.TorchRange));
        this.torchActiveTime = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(WispOption.TorchActiveTime));
        this.torchBlackOutTime = OptionManager.Instance.GetValue<float>(
            GetRoleOptionId(WispOption.BlackOutTime));
		tryAddWispSystem();
	}

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
		return;
    }

    protected override void CreateSpecificOption(OptionFactory factory)
    {
		factory.CreateIntOption(
            WispOption.WinNum,
            0, -5, 5, 1);
		factory.CreateIntOption(
            WispOption.TorchAbilityNum,
            1, 0, 5, 1);
		factory.CreateIntOption(
            WispOption.TorchNum,
            1, 1, 5, 1);
		factory.CreateFloatOption(
            WispOption.TorchRange,
            1.6f, 1.0f, 5.0f, 0.1f);
		factory.CreateFloatOption(
            WispOption.TorchActiveTime,
            10.0f, 5.0f, 60.0f, 0.1f,
            format: OptionUnit.Second);
		factory.CreateFloatOption(
            WispOption.BlackOutTime,
            10.0f, 2.5f, 30.0f, 0.1f,
            format: OptionUnit.Second);
		GhostRoleAbilityFactory.CreateButtonOption(factory);
	}

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.WispTorch,
			(writer) =>
			{
				writer.Write((byte)WispTorchSystem.Ops.SetTorch);
				writer.Write(CachedPlayerControl.LocalPlayer.PlayerId);
			});
    }

	private void tryAddWispSystem()
	{
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.WispTorch,
			() => new WispTorchSystem(this.torchNum, this.range, this.torchActiveTime, this.torchBlackOutTime));
	}
}

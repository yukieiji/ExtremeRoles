using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.Ability.ModeSwitcher;
using ExtremeRoles.Module.Ability.Factory;
using ExtremeRoles.Module.Ability.AutoActivator;
using ExtremeRoles.Module.Ability.Behavior;
using ExtremeRoles.Module.Ability.Behavior.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance.Il2Cpp;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factory.Old.OldAutoParentSetOptionCategoryFactory;
using ExtremeRoles.Module.CustomOption.OLDS;
using ExtremeRoles.Module.CustomOption.Factory.Old;
using ExtremeRoles.Module.CustomOption.Interfaces.Old;


#nullable enable

namespace ExtremeRoles.Roles.Combination;

public sealed class Kids : GhostAndAliveCombinationRoleManagerBase
{
    public const string Name = "Kids";

    public Kids() : base(
		CombinationRoleType.Kids,
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
            if (rolePlayerId == PlayerControl.LocalPlayer.PlayerId)
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

    public sealed class DelinquentAbilityBehavior : BehaviorBase
    {
        public int AbilityCount { get; private set; }
		public AbilityType CurAbility
		{
			get => this.switcher.Current;
			set
			{
				this.switcher.Switch(value);
			}
		}

        private bool isUpdate;

        private TextMeshPro? abilityCountText;
        private Func<bool> useAbility;
        private Func<bool> canUse;

        private readonly GraphicSwitcher<AbilityType> switcher;

        public DelinquentAbilityBehavior(
            GraphicMode<AbilityType> scribeMode,
            GraphicMode<AbilityType> bombMode,
            Func<bool> canUse,
            Func<bool> useAbility) : base(
                scribeMode.Graphic.Text,
                scribeMode.Graphic.Img)
        {
            this.useAbility = useAbility;
            this.canUse = canUse;

            this.switcher = new GraphicSwitcher<AbilityType>(this, scribeMode, bombMode);
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
            this.abilityCountText!.text = Tr.GetString(
				"scribeText", this.AbilityCount);
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
		RoleCore.BuildNeutral(
			ExtremeRoleId.Delinquent,
			ColorPalette.KidsYellowGreen),
        false, false, false, false,
        tab: OptionTab.CombinationTab)
    { }

    public static void Ability(ref MessageReader reader)
    {
		AbilityType abilityType = (AbilityType)reader.ReadByte();
		byte playerId = reader.ReadByte();

		if (!ExtremeRoleManager.TryGetSafeCastedRole<Delinquent>(playerId, out var delinquent))
		{
			return;
		}
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
		rend.sprite = randomSprite;

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
                new (
					AbilityType.Scribe,
					new ButtonGraphic(
						Tr.GetString("scribble"),
						randomSprite)
				),
                new (
					AbilityType.SelfBomb,
					new ButtonGraphic(
						Tr.GetString("selfBomb"),
						UnityObjectLoader.LoadFromResources<Sprite>(ObjectPath.Bomb))
				),
                this.IsAbilityUse,
                this.UseAbility),
            new RoleButtonActivator(),
			KeyCode.F);

		((IRoleAbility)(this)).RoleAbilityInit();

		if (this.Button?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
				this.Loader.GetValue<RoleAbilityCommonOption, int>(
                    RoleAbilityCommonOption.AbilityCount));
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
                    PlayerControl.LocalPlayer, this, this.range) != null,
            _ => true
        };
    }

    public void ResetOnMeetingEnd(NetworkedPlayerInfo? exiledPlayer = null)
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
            caller.WriteByte(PlayerControl.LocalPlayer.PlayerId);
        }
        switch (this.curAbilityType)
        {
            case AbilityType.Scribe:
                setScibe(PlayerControl.LocalPlayer, this);
                break;
            case AbilityType.SelfBomb:
                setBomb(this);
                break;
            default:
                break;
        }
        return true;
    }

    protected override void CreateSpecificOption(OldAutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(factory, 7, 20);
        factory.CreateFloatOption(
            DelinqentOption.Range,
            3.6f, 1.0f, 5.0f, 0.1f);
    }

    protected override void RoleSpecificInit()
    {
        this.curAbilityType = AbilityType.Scribe;

        this.abilityCount = 0;

		var loader = this.Loader;
        this.range = loader.GetValue<DelinqentOption, float>(
            DelinqentOption.Range);

        if (this.Button?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
				loader.GetValue<RoleAbilityCommonOption, int>(
                    RoleAbilityCommonOption.AbilityCount));
        }

        this.canAssignWisp = true;
    }

	private static Sprite randomSprite
		=> UnityObjectLoader.LoadFromResources(
			CombinationRoleType.Kids,
			$"{RandomGenerator.Instance.Next(0, maxImageNum)}");

}

public sealed class Wisp : GhostRoleBase, IGhostRoleWinable, ICombination
{
	public MultiAssignRoleBase.OptionOffsetInfo? OffsetInfo { get; set; }
	public override IOldOptionLoader Loader
	{
		get
		{
			if (OffsetInfo is null ||
				!OldOptionManager.Instance.TryGetCategory(
					this.Tab,
					ExtremeRoleManager.GetCombRoleGroupId(this.OffsetInfo.RoleId),
					out var cate))
			{
				throw new ArgumentException("Can't find category");
			}
			return new OldOptionLoadWrapper(cate, this.OffsetInfo.IdOffset);
		}
	}


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
        OptionTab.CombinationTab)
    { }

	public static Sprite TorchSprite
		=> UnityObjectLoader.LoadFromResources(
			CombinationRoleType.Kids, "Torch");

    public void SetAbilityNum(int abilityNum)
    {
		if (this.Button?.Behavior is ICountBehavior behavior)
		{
			behavior.SetAbilityCount(abilityNum + this.abilityNum);
		}
    }


    public bool IsWin(
        GameOverReason reason,
        NetworkedPlayerInfo ghostRolePlayer) => this.system != null && this.system.IsWin(this);

    public void SetWinPlayerNum(byte rolePlayerId)
    {
        foreach (NetworkedPlayerInfo player in
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
			TorchSprite,
            this.IsReportAbility(),
            () => true,
            () => true,
            this.UseAbility,
            () => { }, true);
        this.ButtonInit();
        this.Button.SetLabelToCrewmate();

		tryAddWispSystem();
	}

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    public override void Initialize()
    {
		var loader = this.Loader;
        this.abilityNum = loader.GetValue<WispOption, int>(
            WispOption.TorchAbilityNum);
        this.winNum = loader.GetValue<WispOption, int>(
            WispOption.WinNum);
        this.torchNum = loader.GetValue<WispOption, int>(
            WispOption.TorchNum);
        this.range = loader.GetValue<WispOption, float>(
            WispOption.TorchRange);
        this.torchActiveTime = loader.GetValue<WispOption, float>(
            WispOption.TorchActiveTime);
        this.torchBlackOutTime = loader.GetValue<WispOption, float>(
            WispOption.BlackOutTime);
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
				writer.Write(PlayerControl.LocalPlayer.PlayerId);
			});
    }

	private void tryAddWispSystem()
	{
		this.system = ExtremeSystemTypeManager.Instance.CreateOrGet(
			ExtremeSystemType.WispTorch,
			() => new WispTorchSystem(this.torchNum, this.range, this.torchActiveTime, this.torchBlackOutTime));
	}
}

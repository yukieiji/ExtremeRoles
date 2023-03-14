using System;
using System.Collections.Generic;
using System.Linq;

using Hazel;
using UnityEngine;

using AmongUs.GameOptions;
using TMPro;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityFactory;
using ExtremeRoles.Module.AbilityBehavior;
using ExtremeRoles.Module.AbilityModeSwitcher;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.GhostRoles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Module.ButtonAutoActivator;

namespace ExtremeRoles.Roles.Combination;

public sealed class Kids : GhostAndAliveCombinationRoleManagerBase
{
    public const string Name = "Kids";

    public enum AbilityType : byte
    {
        Scribe,
        SelfBomb,
        SetTorch,
        PickUpTorch,
        RemoveTorch,
        RepairTorchVision,
        ResetMeeting
    }

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

    protected override void CommonInit()
    {
        base.CommonInit();
        Wisp.StateInit();
    }

    public static void Ability(ref MessageReader reader)
    {
        AbilityType abilityType = (AbilityType)reader.ReadByte();

        switch (abilityType)
        {
            case AbilityType.Scribe:
            case AbilityType.SelfBomb:
                byte playerId = reader.ReadByte();
                PlayerControl rolePlayer = Player.GetPlayerControlById(playerId);
                Delinquent.Ability(abilityType, rolePlayer);
                break;
            case AbilityType.SetTorch:
            case AbilityType.PickUpTorch:
            case AbilityType.RemoveTorch:
            case AbilityType.RepairTorchVision:
            case AbilityType.ResetMeeting:
                Wisp.Ability(ref reader, abilityType);
                break;
            default:
                break;
        }
    }
}

public sealed class Delinquent : MultiAssignRoleBase, IRoleAbility
{
    public override bool IsAssignGhostRole => this.canAssignWisp;


    public sealed class DelinquentAbilityBehavior : AbilityBehaviorBase
    {
        public int AbilityCount { get; private set; }
        public Kids.AbilityType CurAbility { get; private set; }

        private bool isUpdate;

        private TextMeshPro abilityCountText;
        private Func<bool> useAbility;
        private Func<bool> canUse;

        private GraphicSwitcher<Kids.AbilityType> switcher;

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

            this.switcher = new GraphicSwitcher<Kids.AbilityType>(this);
            this.switcher.Add(Kids.AbilityType.Scribe, scribeMode);
            this.switcher.Add(Kids.AbilityType.SelfBomb, bombMode);
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
            if (updateBomb && this.CurAbility == Kids.AbilityType.Scribe)
            {
                this.AbilityCount = 1;
                this.CurAbility = Kids.AbilityType.SelfBomb;
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
            this.abilityCountText.text = string.Format(
                Translation.GetString("scribeText"), this.AbilityCount);
        }
    }

    public ExtremeAbilityButton Button
    { 
        get => this.abilityButton; 
        set
        {
            this.abilityButton = value;
        }
    }

    public bool WinCheckEnable => this.isWinCheck;
    public float Range => this.range;

    public enum DelinqentOption
    {
        Range,
    }

    public int WispAbilityNum => this.abilityCount;

    private Kids.AbilityType curAbilityType;
    private float range;
    private bool isWinCheck;

    private int abilityCount = 0;

    private ExtremeAbilityButton abilityButton;

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

    public static void Ability(Kids.AbilityType abilityType, PlayerControl player)
    {
        Delinquent delinquent = 
            ExtremeRoleManager.GetSafeCastedRole<Delinquent>(player.PlayerId);
        switch (abilityType)
        {
            case Kids.AbilityType.Scribe:
                setScibe(player, delinquent);
                break;
            case Kids.AbilityType.SelfBomb:
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

        this.RoleAbilityInit();

        if (this.abilityButton?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                OptionHolder.AllOption[GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)].GetValue());
        }
    }

    public bool IsAbilityUse()
    {
        if (!(this.abilityButton?.Behavior is DelinquentAbilityBehavior behavior))
        {
            return false;
        }

        this.curAbilityType = behavior.CurAbility;

        return this.curAbilityType switch
        {
            Kids.AbilityType.Scribe =>
                this.IsCommonUse(),
            Kids.AbilityType.SelfBomb =>
                Player.GetClosestPlayerInRange(
                    CachedPlayerControl.LocalPlayer, this, this.range) != null,
            _ => true
        };
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo exiledPlayer = null)
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
            case Kids.AbilityType.Scribe:
                setScibe(CachedPlayerControl.LocalPlayer, this);
                break;
            case Kids.AbilityType.SelfBomb:
                setBomb(this);
                break;
            default:
                break;
        }
        return true;
    }

    protected override void CreateSpecificOption(IOption parentOps)
    {
        this.CreateAbilityCountOption(parentOps, 7, 20);
        CreateFloatOption(
            DelinqentOption.Range,
            3.6f, 1.0f, 5.0f, 0.1f,
            parentOps);
    }

    protected override void RoleSpecificInit()
    {
        this.curAbilityType = Kids.AbilityType.Scribe;

        this.abilityCount = 0;

        this.range = OptionHolder.AllOption[
            GetRoleOptionId(DelinqentOption.Range)].GetValue();
        
        this.RoleAbilityInit();
        if (this.abilityButton?.Behavior is DelinquentAbilityBehavior behavior)
        {
            behavior.SetAbilityCount(
                OptionHolder.AllOption[GetRoleOptionId(
                    RoleAbilityCommonOption.AbilityCount)].GetValue());
        }

        this.canAssignWisp = true;
    }
    
}

public sealed class Wisp : GhostRoleBase, IGhostRoleWinable
{
    public sealed class Torch : IMeetingResetObject
    {
        private GameObject body;

        public Torch(float range, Vector2 pos)
        {
            this.body = new GameObject("Torch");
            this.body.transform.position = new Vector3(
                pos.x, pos.y, (pos.y / 1000f));
            TorchBehavior torch = this.body.AddComponent<TorchBehavior>();
            torch.SetRange(range);
            this.body.SetActive(true);
        }

        public void Clear()
        {
            GameObject.Destroy(this.body);
        }
    }

    public sealed class TorchManager : IMeetingResetObject, IUpdatableObject
    {
        private uint id = 0;
        private int placedControlId = 0;
        private float timer = float.MaxValue;
        private float blackOutTime = float.MinValue;
        private List<Torch> torch = new List<Torch>();

        public TorchManager(
            uint id,
            Wisp wisp)
        {
            this.id = id;
            this.placedControlId = wisp.GameControlId;
            this.timer = wisp.torchActiveTime;
            this.blackOutTime = wisp.torchBlackOutTime;
            this.torch.Clear();
            this.SetTorch(wisp.torchNum, wisp.range);
        }

        public void Clear()
        {
            foreach (var torch in this.torch)
            {
                torch.Clear();
            }
            this.torch.Clear();
        }

        public void Update(int index)
        {
            this.timer -= Time.deltaTime;
            if (this.timer <= 0.0f)
            {
                using (var caller = RPCOperator.CreateCaller(
                    RPCOperator.Command.KidsAbility))
                {
                    caller.WriteByte((byte)Kids.AbilityType.RemoveTorch);
                    caller.WriteUInt(this.id);
                    caller.WriteInt(this.placedControlId);
                    caller.WriteFloat(this.blackOutTime);
                }
                RemoveTorch(this.id, this.placedControlId, this.blackOutTime);
                ExtremeRolesPlugin.ShipState.RemoveUpdateObjectAt(index);
            }
        }

        public void SetTorch(int num, float range)
        {
            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId);
            int playerNum = CachedPlayerControl.AllPlayerControls.Count;
            int clampedNum = Math.Clamp(num, 0, playerNum);
            ShipStatus ship = CachedShipStatus.Instance;
            IEnumerable<CachedPlayerControl> target =   
                CachedPlayerControl.AllPlayerControls.OrderBy(
                    x => RandomGenerator.Instance.Next()).Take(clampedNum);

            foreach (CachedPlayerControl player in target)
            {
                byte playerId = player.PlayerId;

                List<Vector2> placePos = new List<Vector2>();

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    placePos = ExtremeRolesPlugin.Compat.ModMap.GetSpawnPos(
                        playerId);
                }
                else
                {
                    switch (mapId)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            Vector2 baseVec = Vector2.up;
                            baseVec = baseVec.Rotate(
                                (float)(playerId - 1) * (360f / (float)playerNum));
                            Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(
                                0f, 0.3636f);
                            placePos.Add(ship.InitialSpawnCenter + offset);
                            placePos.Add(ship.MeetingSpawnCenter + offset);
                            break;
                        case 4:
                            placePos = GameSystem.GetAirShipRandomSpawn();
                            break;
                    }
                }
                var newTorch = new Torch(range, placePos[
                    RandomGenerator.Instance.Next(0, placePos.Count)]);
                ExtremeRolesPlugin.ShipState.AddMeetingResetObject(newTorch);
                this.torch.Add(newTorch);
            }
        }
    }

    public sealed class WispBlackOuter : IMeetingResetObject, IUpdatableObject
    {
        private float timer = float.MaxValue;
        private float maxTime = float.MaxValue;

        public WispBlackOuter(float time)
        {
            // ここは全員呼ばれる
            VisionComputer.Instance.SetModifier(
                VisionComputer.Modifier.WispLightOff);
            this.maxTime = time;
            this.timer = time;
        }

        public void ResetTimer()
        {
            this.timer = this.maxTime;
        }

        public void Clear()
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.KidsAbility))
            {
                caller.WriteByte((byte)Kids.AbilityType.RepairTorchVision);
            }
            RepairVision();
        }

        public void Update(int index)
        {
            this.timer -= Time.deltaTime;
            if (this.timer <= 0.0f)
            {
                ExtremeRolesPlugin.ShipState.RemoveUpdateObjectAt(index);
            }
        }
    }

    public sealed class WispState
    {
        private uint torchId;
        private Dictionary<uint, TorchManager> placedTorch = new Dictionary<uint, TorchManager>();
        private WispBlackOuter blackOuter = null;
        private HashSet<byte> torchHavePlayer = new HashSet<byte>();
        private Dictionary<int, int> affectedPlayerNum = new Dictionary<int, int>();

        public WispState()
        {
            this.torchId = 0;
            this.blackOuter = null;
        }

        public bool IsWin(Wisp wisp)
        {
            int gameControlId = wisp.GameControlId;
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                gameControlId = PlayerStatistics.SameNeutralGameControlId;
            }
            if (this.affectedPlayerNum.TryGetValue(gameControlId, out int playerNum))
            {
                return playerNum >= wisp.winNum;
            }
            else
            {
                return false;
            }
        }

        public int CurAffectedPlayerNum(Wisp wisp)
        {
            int gameControlId = wisp.GameControlId;
            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                gameControlId = PlayerStatistics.SameNeutralGameControlId;
            }

            if (this.affectedPlayerNum.TryGetValue(gameControlId, out int playerNum))
            {
                return playerNum;
            }
            else
            {
                return 0;
            }
        }

        public void SetTorch(Wisp wisp)
        {
            var torch = new TorchManager(this.torchId, wisp);
            ExtremeRolesPlugin.ShipState.AddUpdateObject(torch);
            ExtremeRolesPlugin.ShipState.AddMeetingResetObject(torch);
            this.placedTorch.Add(
                this.torchId, torch);
            this.torchId++;
        }

        public bool HasTorch(byte playerId) => this.torchHavePlayer.Contains(playerId);

        public void PickUpTorch(byte playerId)
        {
            this.torchHavePlayer.Add(playerId);
        }

        public void RemoveTorch(uint id, int gameControlId, float time)
        {
            if (this.placedTorch.TryGetValue(id, out TorchManager torch))
            {
                torch.Clear();
               
                if (this.blackOuter == null)
                {
                    this.blackOuter = new WispBlackOuter(time);
                    ExtremeRolesPlugin.ShipState.AddUpdateObject(this.blackOuter);
                    ExtremeRolesPlugin.ShipState.AddMeetingResetObject(this.blackOuter);
                }
                this.blackOuter.ResetTimer();
                UpdateAffectedPlayerNum(gameControlId);
            }
        }

        public void RepairVision()
        {
            VisionComputer.Instance.ResetModifier();
            this.blackOuter = null;
        }

        public void ResetMeeting()
        {
            this.torchHavePlayer.Clear();
            this.blackOuter?.Clear();
            this.placedTorch.Clear();
            RepairVision();
        }

        public void UpdateAffectedPlayerNum(int gameControlId)
        {
            int playerNum = 0;
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
            {
                if (player.IsDead || 
                    player.Disconnected ||
                    HasTorch(player.PlayerId)) { continue; }
                ++playerNum;
            }

            if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
            {
                gameControlId = PlayerStatistics.SameNeutralGameControlId;
            }

            this.affectedPlayerNum[gameControlId] = 
                this.affectedPlayerNum.TryGetValue(gameControlId, out int result) ? 
                result + playerNum : playerNum;
        }

        public void Initialize()
        {
            this.torchId = 0;
            this.affectedPlayerNum.Clear();
            ResetMeeting();
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
    private static WispState state = new WispState();

    public Wisp() : base(
        false, ExtremeRoleType.Neutral,
        ExtremeGhostRoleId.Wisp,
        ExtremeGhostRoleId.Wisp.ToString(),
        ColorPalette.KidsYellowGreen,
        OptionTab.Combination)
    { }

    public static void StateInit()
    {
        state.Initialize();
    }

    public static void Ability(ref MessageReader reader, Kids.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case Kids.AbilityType.PickUpTorch:
                PickUpTorch(reader.ReadByte());
                break;
            case Kids.AbilityType.RemoveTorch:
                uint id = reader.ReadUInt32();
                int controlId = reader.ReadInt32();
                float time = reader.ReadSingle();
                RemoveTorch(id, controlId, time);
                break;
            case Kids.AbilityType.RepairTorchVision:
                RepairVision();
                break;
            case Kids.AbilityType.ResetMeeting:
                resetMeeting();
                break;
            default:
                break;
        }
    }

    public static bool HasTorch(byte playerId) => state.HasTorch(playerId);

    public static void RepairVision()
    {
        state.RepairVision();
    }

    public static void SetTorch(byte playerId)
    {
        Wisp wisp = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Wisp>(playerId);
        if (wisp != null)
        {
            state.SetTorch(wisp);
        }
    }

    public static void PickUpTorch(byte playerId)
    {
        state.PickUpTorch(playerId);
    }

    public static void RemoveTorch(uint id, int controlId, float blackOutTime)
    {
        state.RemoveTorch(id, controlId, blackOutTime);
    }

    public void SetAbilityNum(int abilityNum)
    {
        ((AbilityCountBehavior)this.Button.Behavior).SetAbilityCount(abilityNum + this.abilityNum);
    }
    private static void resetMeeting()
    {
        state.ResetMeeting();
    }

    public bool IsWin(
        GameOverReason reason,
        GameData.PlayerInfo ghostRolePlayer) => state.IsWin(this);

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
    }

    public override string GetFullDescription()
    {
        return string.Format(
            base.GetFullDescription(),
            this.winNum,
            state.CurAffectedPlayerNum(this));
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
            abilityCall, true);
        this.ButtonInit();
        this.Button.SetLabelToCrewmate();
    }

    public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

    public override void Initialize()
    {
        this.abilityNum = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.TorchAbilityNum)].GetValue();
        this.winNum = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.WinNum)].GetValue();
        this.torchNum = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.TorchNum)].GetValue();
        this.range = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.TorchRange)].GetValue();
        this.torchActiveTime = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.TorchActiveTime)].GetValue();
        this.torchBlackOutTime = OptionHolder.AllOption[
            GetRoleOptionId(WispOption.BlackOutTime)].GetValue();
    }

    protected override void OnMeetingEndHook()
    {
        return;
    }

    protected override void OnMeetingStartHook()
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.KidsAbility))
        {
            caller.WriteByte((byte)Kids.AbilityType.ResetMeeting);
        }
    }

    protected override void CreateSpecificOption(IOption parentOps)
    {
        CreateIntOption(
            WispOption.WinNum,
            0, -5, 5, 1, parentOps);
        CreateIntOption(
            WispOption.TorchAbilityNum,
            1, 0, 5, 1, parentOps);
        CreateIntOption(
            WispOption.TorchNum,
            1, 1, 5, 1, parentOps);
        CreateFloatOption(
            WispOption.TorchRange,
            1.6f, 1.0f, 5.0f, 0.1f, parentOps);
        CreateFloatOption(
            WispOption.TorchActiveTime,
            10.0f, 5.0f, 60.0f, 0.1f, parentOps,
            format: OptionUnit.Second);
        CreateFloatOption(
            WispOption.BlackOutTime,
            10.0f, 2.5f, 30.0f, 0.1f, parentOps,
            format: OptionUnit.Second);
        this.CreateButtonOption(parentOps);
    }

    protected override void UseAbility(RPCOperator.RpcCaller caller)
    {
        caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
    }

    private void abilityCall()
    {
        state.SetTorch(this);
    }
}

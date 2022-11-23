using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.ExtremeShipStatus;


using GhostAbilityButton = ExtremeRoles.Module.AbilityButton.GhostRoles.ReusableAbilityButton;
using RoleButtonBase = ExtremeRoles.Module.AbilityButton.Roles.RoleAbilityButtonBase;

namespace ExtremeRoles.Roles.Combination
{
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
            RepairTorchVison,
        }

        public Kids() : base(
            Name, new Color(255f, 255f, 255f), 2,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Delinquent());

            this.CombGhostRole.Add(
                ExtremeRoleId.Delinquent, new Wisp());
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
                case AbilityType.RepairTorchVison:
                    Wisp.Ability(ref reader, abilityType);
                    break;
                default:
                    break;
            }
        }
    }

    public sealed class Delinquent : MultiAssignRoleBase, IRoleAbility
    {
        public sealed class DelinquentAbilityButton : RoleButtonBase
        {
            public int CurAbilityNum
            {
                get => this.abilityNum;
            }
            public Kids.AbilityType CurAbility => this.curAbility;

            public Kids.AbilityType curAbility;

            private int abilityNum = 0;
            private TMPro.TextMeshPro abilityCountText = null;
            private Sprite bombScribe;

            public DelinquentAbilityButton(
                string buttonText,
                Func<bool> ability,
                Func<bool> canUse,
                Sprite scribeSprite,
                Sprite bombSprite) : base(
                    buttonText,
                    ability,
                    canUse,
                    scribeSprite,
                    new Vector3(-1.8f, -0.06f, 0),
                    null,
                    null,
                    KeyCode.F, false)
            {
                this.curAbility = Kids.AbilityType.Scribe;
                this.bombScribe = bombSprite;
                this.abilityCountText = GameObject.Instantiate(
                    this.Button.cooldownTimerText,
                    this.Button.cooldownTimerText.transform.parent);
                updateAbilityInfoText();
                this.abilityCountText.enableWordWrapping = false;
                this.abilityCountText.transform.localScale = Vector3.one * 0.5f;
                this.abilityCountText.transform.localPosition += new Vector3(-0.05f, 0.65f, 0);
            }

            public void UpdateAbilityCount(int newCount)
            {
                this.abilityNum = newCount + 1;
                this.updateAbilityInfoText();
            }

            protected override void AbilityButtonUpdate()
            {
                if (this.CanUse() && this.abilityNum > 0)
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.EnabledColor;
                    this.Button.graphic.material.SetFloat("_Desat", 0f);
                }
                else
                {
                    this.Button.graphic.color = this.Button.buttonLabelText.color = Palette.DisabledClear;
                    this.Button.graphic.material.SetFloat("_Desat", 1f);
                }
                if (this.abilityNum == 0)
                {
                    Button.SetCoolDown(0, this.CoolTime);
                    return;
                }
                if (this.Timer >= 0)
                {
                    bool abilityOn = this.IsHasCleanUp() && IsAbilityOn;

                    PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

                    if (abilityOn ||
                        localPlayer.IsKillTimerEnabled ||
                        localPlayer.ForceKillTimerContinue)
                    {
                        this.Timer -= Time.deltaTime;
                    }
                    if (abilityOn)
                    {
                        if (!this.AbilityCheck())
                        {
                            this.Timer = 0;
                            this.IsAbilityOn = false;
                        }
                    }
                }

                if (this.abilityNum > 0)
                {
                    Button.SetCoolDown(
                        this.Timer,
                        (this.IsHasCleanUp() && this.IsAbilityOn) ? this.AbilityActiveTime : this.CoolTime);
                }
            }

            protected override void OnClickEvent()
            {
                if (this.CanUse() &&
                    this.Timer < 0f &&
                    this.abilityNum > 0 &&
                    !this.IsAbilityOn)
                {
                    Button.graphic.color = this.DisableColor;

                    if (this.UseAbility())
                    {
                        this.updateAbility();
                        this.ResetCoolTimer();
                    }
                }
            }

            private void updateAbility()
            {
                --this.abilityNum;
                bool updateBomb = this.abilityNum > 1;
                if (updateBomb)
                {
                    this.ButtonSprite = this.bombScribe;
                    this.curAbility = Kids.AbilityType.SelfBomb;
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
            }

            private void updateAbilityInfoText()
            {
                this.abilityCountText.text = Translation.GetString("scribeText") + 
                    string.Format(
                        Translation.GetString(OptionUnit.Shot.ToString()), this.abilityNum);
            }

        }

        public RoleButtonBase Button
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

        private Kids.AbilityType curAbilityType;
        private float range;
        private bool isWinCheck;

        private RoleButtonBase abilityButton;

        public Delinquent() : base(
            ExtremeRoleId.Delinquent,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Delinquent.ToString(),
            Palette.White,
            false, false, false, false,
            tab: OptionTab.Combination)
        { }

        public static void Ability(Kids.AbilityType abilityType, PlayerControl player)
        {
            switch (abilityType)
            {
                case Kids.AbilityType.Scribe:
                    setScibe(player);
                    break;
                case Kids.AbilityType.SelfBomb:
                    setBomb(
                        ExtremeRoleManager.GetSafeCastedRole<Delinquent>(
                            player.PlayerId));
                    break;
            }
        }

        private static void setScibe(PlayerControl player)
        {
            GameObject obj = new GameObject("Scribe");
            obj.transform.position = player.transform.position;
            SpriteRenderer rend = obj.AddComponent<SpriteRenderer>();
            rend.sprite = Loader.CreateSpriteFromResources(
                Path.TestButton);
        }
        private static void setBomb(Delinquent delinquent)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                delinquent.isWinCheck = true;
            }
        }

        public void DisableWinCheck()
        {
            this.isWinCheck = false;
        }

        public void CreateAbility()
        {
            this.Button = new DelinquentAbilityButton(
                Translation.GetString("scribble"),
                this.UseAbility,
                this.IsAbilityUse,
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                Loader.CreateSpriteFromResources(
                    Path.TestButton));
            this.RoleAbilityInit();
        }

        public bool IsAbilityUse()
        {
            this.curAbilityType = ((DelinquentAbilityButton)this.abilityButton).CurAbility;

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

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
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
                    setScibe(CachedPlayerControl.LocalPlayer);
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
            CreateFloatOption(
                DelinqentOption.Range,
                3.6f, 1.0f, 5.0f, 0.1f,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.curAbilityType = Kids.AbilityType.Scribe;
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(DelinqentOption.Range)].GetValue();
            this.RoleAbilityInit();
        }
        
    }

    public sealed class Wisp : GhostRoleBase
    {
        public sealed class Torch : IMeetingResetObject
        {
            private GameObject body;

            public Torch(Vector3 pos)
            {
                this.body = new GameObject("Torch");
                this.body.transform.position = pos;
                this.body.AddComponent<TorchBehavior>();
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
            private float timer = float.MaxValue;
            private float blackOutTime = float.MinValue;
            private List<Torch> torch = new List<Torch>();

            public TorchManager(uint id, float activeTime, float blackOutTime)
            {
                this.id = id;
                this.timer = activeTime;
                this.blackOutTime = blackOutTime;
                this.torch.Clear();
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
                        caller.WriteFloat(this.blackOutTime);
                    }
                    RemoveTorch(this.id, this.blackOutTime);
                    ExtremeRolesPlugin.ShipState.RemoveUpdateObjectAt(index);
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
                ExtremeRolesPlugin.ShipState.SetVison(
                    ExtremeShipStatus.ForceVisonType.WispLightOff);
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
                    caller.WriteByte((byte)Kids.AbilityType.RepairTorchVison);
                }
                RepairVison();
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

            public WispState()
            {
                this.torchId = 0;
                this.blackOuter = null;
            }

            public void SetTorch(float activeTime, float blackOutTime)
            {
                var torch = new TorchManager(this.torchId, activeTime, blackOutTime);
                ExtremeRolesPlugin.ShipState.AddUpdateObject(torch);
                ExtremeRolesPlugin.ShipState.AddMeetingResetObject(torch);
                this.placedTorch.Add(
                    this.torchId, torch);
                this.torchId++;
            }

            public void PickUpTorch(byte playerId)
            {
                this.torchHavePlayer.Add(playerId);
            }

            public void RemoveTorch(uint id, float time)
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
                }
            }

            public void RepairVison()
            {
                ExtremeRolesPlugin.ShipState.SetVison(
                    ExtremeShipStatus.ForceVisonType.None);
            }

            public void ResetMeeting()
            {
                this.torchHavePlayer.Clear();
                this.blackOuter.Clear();
                this.placedTorch.Clear();
                this.blackOuter = null;
            }

            public void Initialize()
            {
                this.torchId = 0;
                ResetMeeting();
            }
        }

        public enum WispOption
        {
            TorchRange,
            TorchActiveTime,
            BlackOutTime,
        }

        private float torchActiveTime;
        private float torchBlackOutTime;
        private static WispState state = new WispState();

        public Wisp() : base(
            false, ExtremeRoleType.Neutral,
            ExtremeGhostRoleId.Wisp,
            ExtremeGhostRoleId.Wisp.ToString(),
            Palette.White,
            OptionTab.Combination)
        { }

        public static void Ability(ref MessageReader reader, Kids.AbilityType abilityType)
        {
            switch (abilityType)
            {
                case Kids.AbilityType.SetTorch:
                    var wisp = ExtremeGhostRoleManager.GetSafeCastedGhostRole<Wisp>(
                        reader.ReadByte());
                    SetTorch(wisp);
                    break;
                case Kids.AbilityType.PickUpTorch:
                    PickUpTorch(reader.ReadByte());
                    break;
                case Kids.AbilityType.RemoveTorch:
                    uint id = reader.ReadUInt32();
                    float time = reader.ReadSingle();
                    RemoveTorch(id, time);
                    break;
                case Kids.AbilityType.RepairTorchVison:
                    RepairVison();
                    break;
                default:
                    break;
            }
        }

        public static void RepairVison()
        {
            state.RepairVison();
        }

        public static void SetTorch(Wisp wisp)
        {
            state.SetTorch(wisp.torchActiveTime, wisp.torchBlackOutTime);
        }

        public static void PickUpTorch(byte playerId)
        {
            state.PickUpTorch(playerId);
        }

        public static void RemoveTorch(uint id, float blackOutTime)
        {
            state.RemoveTorch(id, blackOutTime);
        }

        public override void CreateAbility()
        {
            this.Button = new GhostAbilityButton(
                AbilityType.WispSetTorch,
                this.UseAbility,
                null,
                null,
                Loader.CreateSpriteFromResources(
                    Path.CarrierCarry),
                this.DefaultButtonOffset,
                rpcHostCallAbility: abilityCall);
            this.ButtonInit();
            this.Button.SetLabelToCrewmate();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            this.torchActiveTime = OptionHolder.AllOption[
                GetRoleOptionId(WispOption.TorchActiveTime)].GetValue();
            this.torchBlackOutTime = OptionHolder.AllOption[
                GetRoleOptionId(WispOption.BlackOutTime)].GetValue();
        }

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            return;
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            CreateFloatOption(
                WispOption.TorchRange,
                1.6f, 1.0f, 5.0f, 0.1f, parentOps);
            CreateFloatOption(
                WispOption.TorchActiveTime,
                15.0f, 5.0f, 60.0f, 0.1f, parentOps);
            CreateFloatOption(
                WispOption.BlackOutTime,
                15.0f, 5.0f, 30.0f, 0.1f, parentOps);
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        {
            caller.WriteByte((byte)Kids.AbilityType.SetTorch);
            caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
        }

        private void abilityCall()
        {
            SetTorch(this);
        }
    }
}

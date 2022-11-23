using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class Kids : GhostAndAliveCombinationRoleManagerBase
    {
        public const string Name = "Kids";

        public enum AbilityType : byte
        {
            Scribe,
            SelfBomb
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
            byte playerId = reader.ReadByte();
            AbilityType abilityType = (AbilityType)reader.ReadByte();
            PlayerControl rolePlayer = Player.GetPlayerControlById(playerId);

            switch (abilityType)
            {
                case AbilityType.Scribe:
                case AbilityType.SelfBomb:
                    Delinquent.Ability(abilityType, rolePlayer);
                    break;
                default:
                    break;
            }
        }
    }

    public sealed class Delinquent : MultiAssignRoleBase, IRoleAbility
    {
        public sealed class DelinquentAbilityButton : RoleAbilityButtonBase
        {
            public int CurAbilityNum
            {
                get => this.abilityNum;
            }

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

        public RoleAbilityButtonBase Button
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

        private RoleAbilityButtonBase abilityButton;

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
                    GameObject obj = new GameObject("Scribe");
                    obj.transform.position = player.transform.position;
                    SpriteRenderer rend  = obj.AddComponent<SpriteRenderer>();
                    rend.sprite = Loader.CreateSpriteFromResources(
                        Path.TestButton);
                    break;
                case Kids.AbilityType.SelfBomb:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var role = ExtremeRoleManager.GetSafeCastedRole<Delinquent>(player.PlayerId);
                        role.isWinCheck = true;
                    }
                    break;
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

        public bool IsAbilityUse() => this.curAbilityType switch
        {
            Kids.AbilityType.Scribe => 
                this.IsCommonUse(),
            Kids.AbilityType.SelfBomb => 
                Player.GetClosestPlayerInRange(
                    CachedPlayerControl.LocalPlayer, this, this.range) != null,
            _ => true
        };

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
                caller.WriteByte(CachedPlayerControl.LocalPlayer.PlayerId);
                caller.WriteByte((byte)this.curAbilityType);
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
            this.range = OptionHolder.AllOption[
                GetRoleOptionId(DelinqentOption.Range)].GetValue();
            this.RoleAbilityInit();
        }
        
    }

    public sealed class Wisp : GhostRoleBase
    {
        public Wisp() : base(
            false, ExtremeRoleType.Neutral,
            ExtremeGhostRoleId.Wisp,
            ExtremeGhostRoleId.Wisp.ToString(),
            Palette.White,
            OptionTab.Combination)
        { }

        public override void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter()
        {
            throw new System.NotImplementedException();
        }

        public override void Initialize()
        {
            
        }

        public override void ReseOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public override void ReseOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        {
            throw new System.NotImplementedException();
        }
    }
}

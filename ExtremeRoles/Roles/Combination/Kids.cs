using UnityEngine;
using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using Hazel;

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
        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException(); 
            set => throw new System.NotImplementedException();
        }

        public bool WinCheckEnable => this.isWinCheck;
        public float Range => this.range;

        private Kids.AbilityType curAbilityType;
        private float range;
        private bool isWinCheck;

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
            this.CreateAbilityCountButton(
                Translation.GetString("scribble"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton));
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
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new System.NotImplementedException();
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
            
        }

        protected override void RoleSpecificInit()
        {
            
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

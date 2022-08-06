using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public sealed class Umbrer : SingleRoleBase, IRoleAbility, IRoleUpdate
    {
        private sealed class InfectedContainer
        {
            public HashSet<PlayerControl> FirstStage => this.firstStage;
            public HashSet<PlayerControl> FinalStage => this.finalStage;

            private HashSet<PlayerControl> firstStage = new HashSet<PlayerControl>();
            private HashSet<PlayerControl> finalStage = new HashSet<PlayerControl>();

            public InfectedContainer()
            {
                this.firstStage.Clear();
                this.finalStage.Clear();
            }
            public void AddPlayer(PlayerControl player)
            {
                finalStage.Add(player);
            }

            public bool IsAllPlayerInfected()
            {
                if (this.firstStage.Count <= 0) { return false; }

                foreach (GameData.PlayerInfo player in
                    GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (player == null || player?.Object == null) { continue; }
                    if (player.IsDead || player.Disconnected) { continue; }

                    if (!this.firstStage.Contains(player.Object))
                    {
                        return false;
                    }

                }
                return true;
            }

            public bool IsContain(PlayerControl player) =>
                this.firstStage.Contains(player) || this.finalStage.Contains(player);

            public bool IsFirstStage(PlayerControl player) => 
                this.firstStage.Contains(player);

            public void Update()
            {
                removeToHashSet(ref this.firstStage);
                removeToHashSet(ref this.finalStage);
            }

            private void removeToHashSet(ref HashSet<PlayerControl> cont)
            {
                List<PlayerControl> remove = new List<PlayerControl>();

                foreach (PlayerControl player in this.firstStage)
                {
                    if (player == null ||
                        player.Data == null ||
                        player.Data.IsDead ||
                        player.Data.Disconnected)
                    {
                        remove.Add(player);
                    }
                }
                foreach (PlayerControl player in remove)
                {
                    cont.Remove(player);
                }
            }

        }

        public enum UmbrerOption
        {
            
        }

        public RoleAbilityButtonBase Button
        {
            get => this.madmateAbilityButton;
            set
            {
                this.madmateAbilityButton = value;
            }
        }

        private RoleAbilityButtonBase madmateAbilityButton;
        private InfectedContainer container;

        public Umbrer() : base(
            ExtremeRoleId.Umbrer,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Umbrer.ToString(),
            Palette.ImpostorRed,
            false, false, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Helper.Translation.GetString("selfKill"),
                Loader.CreateSpriteFromResources(
                    Path.SucideSprite));
        }

        public bool UseAbility()
        {

            
            return true;
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (CachedShipStatus.Instance == null ||
                this.IsWin ||
                GameData.Instance == null) { return; }
            if (!CachedShipStatus.Instance.enabled) { return; }

            if (this.container.IsAllPlayerInfected())
            {
                this.IsWin = true;
                RPCOperator.RoleIsWin(rolePlayer.PlayerId);
                return;
            }

            this.container.Update();
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (this.Id == targetRole.Id)
            {
                if (OptionHolder.Ship.IsSameNeutralSameWin)
                {
                    return true;
                }
                else
                {
                    return this.IsSameControlId(targetRole);
                }
            }
            else
            {
                return base.IsSameTeam(targetRole);
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            

            this.CreateCommonAbilityOption(parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.container = new InfectedContainer();

            var allOpt = OptionHolder.AllOption;

            this.RoleAbilityInit();
        }
    }
}

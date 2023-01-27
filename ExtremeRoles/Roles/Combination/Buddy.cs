using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class BuddyManager : FlexibleCombinationRoleManagerBase
    {
        public BuddyManager() : base(
            new Buddy(),
            canAssignImposter: false)
        { }

    }

    public sealed class Buddy : MultiAssignRoleBase, IRoleAwake<RoleTypes>, IRoleSpecialSetUp
    {
        public bool IsAwake => this.awake;

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum BuddyOption
        {
            AwakeTaskGage,
        }

        public sealed class BuddyContainer
        {
            private HashSet<GameData.PlayerInfo> buddy = new HashSet<GameData.PlayerInfo>();
            private HashSet<byte> bytedBuddy = new HashSet<byte>();

            public HashSet<GameData.PlayerInfo> PlayerInfo => this.buddy;

            public BuddyContainer()
            {
                this.buddy.Clear();
                this.bytedBuddy.Clear();
            }

            public bool Contains(GameData.PlayerInfo player) => this.buddy.Contains(player);

            public bool Contains(byte playerId) => this.bytedBuddy.Contains(playerId);

            public void Add(GameData.PlayerInfo player)
            {
                this.buddy.Add(player);
                this.bytedBuddy.Add(player.PlayerId);
            }
        }

        private float awakeTaskGage;
        private bool awake;
        private bool awakeHasOtherVision;
        private BuddyContainer buddy;
        private SingleRoleBase hiddeRole = null;

        public Buddy() : base(
            ExtremeRoleId.Buddy,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Buddy.ToString(),
            ColorPalette.BuddyOrange,
            false, true,
            false, false,
            tab: OptionTab.Combination)
        { }

        public string GetFakeOptionString() => "";

        public void IntroBeginSetUp()
        {
            this.buddy = this.getSameBuddy();

            if (IsAwake || 
                !this.CanHasAnotherRole || 
                this.AnotherRole == null) { return; }

            this.hiddeRole = this.AnotherRole;
            this.CanHasAnotherRole = false;
            this.AnotherRole = null;
        }

        public void IntroEndSetUp()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awake &&
                this.buddy != null &&
                rolePlayer != null &&
                rolePlayer.Data != null &&
                rolePlayer.Data.Tasks.Count != 0)
            {
                foreach (GameData.PlayerInfo playerId in this.buddy.PlayerInfo)
                {
                    if (Player.GetPlayerTaskGage(playerId) < this.awakeTaskGage)
                    {
                        if (this.hiddeRole is IRoleAbility abilityRole)
                        {
                            abilityRole.Button.SetActive(false);
                        }
                        return;
                    }
                }
                this.awake = true;
                this.HasOtherVison = this.awakeHasOtherVision;
                if (this.hiddeRole != null)
                {
                    this.AnotherRole = this.hiddeRole;
                    this.CanHasAnotherRole = true;
                    if (this.AnotherRole is IRoleAbility abilityRole)
                    {
                        abilityRole.Button.SetActive(false);
                    }
                    this.hiddeRole = null;
                }
            }
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (IsAwake && this.buddy.Contains(targetPlayerId))
            {
                return ColorPalette.BuddyOrange;
            }
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetColoredRoleName(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetColoredRoleName();
            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    Translation.GetString(RoleTypes.Crewmate.ToString()));
            }
        }

        public override string GetFullDescription()
        {
            if (IsAwake)
            {
                List<string> fullDec = new List<string>();

                foreach (GameData.PlayerInfo player in this.buddy?.PlayerInfo)
                {
                    if (player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                    {
                        continue;
                    }
                    if (fullDec.Count == 0)
                    {
                        fullDec.Add(Translation.GetString("andFirst"));
                    }
                    else
                    {
                        fullDec.Add(Translation.GetString("and"));
                    }
                    fullDec.Add(player.PlayerName);
                }
                return string.Format(
                    base.GetFullDescription(),
                    string.Concat(fullDec));
            }
            else
            {
                return Translation.GetString(
                    $"{RoleTypes.Crewmate}FullDescription");
            }
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (IsAwake)
            {
                return base.GetImportantText(isContainFakeTask);

            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
            }
        }

        public override string GetIntroDescription()
        {
            if (IsAwake)
            {
                List<string> intro = new List<string>();

                foreach (GameData.PlayerInfo player in this.buddy.PlayerInfo)
                {
                    if (player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                    {
                        continue;
                    }
                    if (intro.Count == 0)
                    {
                        intro.Add(Translation.GetString("andFirst"));
                    }
                    else
                    {
                        intro.Add(Translation.GetString("and"));
                    }
                    intro.Add(player.PlayerName);
                }

                return string.Format(
                    base.GetIntroDescription(),
                    string.Concat(intro));
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

        public override string GetRoleTag() => "●";

        public override Color GetNameColor(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetNameColor(isTruthColor);
            }
            else
            {
                return Palette.White;
            }
        }

        public override string GetRolePlayerNameTag(
            SingleRoleBase targetRole, byte targetPlayerId)
        {
            if (IsAwake &&
                this.buddy.Contains(targetPlayerId))
            {
                return Design.ColoedString(
                    ColorPalette.BuddyOrange,
                    $" {GetRoleTag()}");
            }

            return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            CreateIntOption(
                BuddyOption.AwakeTaskGage,
                50, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
               GetRoleOptionId(BuddyOption.AwakeTaskGage)].GetValue() / 100.0f;

            this.awakeHasOtherVision = this.HasOtherVison;

            if (this.awakeTaskGage <= 0.0f)
            {
                this.awake = true;
                this.HasOtherVison = this.awakeHasOtherVision;
            }
            else
            {
                this.awake = false;
                this.HasOtherVison = false;
            }
        }

        private BuddyContainer getSameBuddy()
        {

            BuddyContainer buddy = new BuddyContainer();

            foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
            {
                var player = GameData.Instance.GetPlayerById(playerId);
                if (this.IsSameControlId(role) &&
                    (!player.IsDead || !player.Disconnected))
                {
                    buddy.Add(player);
                }
            }

            return buddy;

        }
    }
}

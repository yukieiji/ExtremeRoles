using System.Collections.Generic;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Roles.Combination;

public sealed class BuddyManager : FlexibleCombinationRoleManagerBase
{
    public BuddyManager() : base(
		CombinationRoleType.Buddy,
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
        private HashSet<NetworkedPlayerInfo> buddy = new HashSet<NetworkedPlayerInfo>();
        private HashSet<byte> bytedBuddy = new HashSet<byte>();

        public HashSet<NetworkedPlayerInfo> PlayerInfo => this.buddy;

        public BuddyContainer()
        {
            this.buddy.Clear();
            this.bytedBuddy.Clear();
        }

        public string GetAllPlayerName()
        {
            List<string> playerName = new List<string>();

            foreach (NetworkedPlayerInfo player in this.PlayerInfo)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    continue;
                }

                int count = playerName.Count;
                if (count > 1)
                {
                    playerName.Add(Tr.GetString("andFirst"));
                }
                else if (count == 1)
                {
                    playerName.Add(Tr.GetString("and"));
                }
                playerName.Add(player.PlayerName);
            }
            return string.Concat(playerName);
        }

        public bool Contains(NetworkedPlayerInfo player) => this.buddy.Contains(player);

        public bool Contains(byte playerId) => this.bytedBuddy.Contains(playerId);

        public void Add(NetworkedPlayerInfo player)
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
		RoleCore.BuildCrewmate(
			ExtremeRoleId.Buddy,
			ColorPalette.BuddyOrange),
        false, true,
        false, false,
        tab: OptionTab.CombinationTab)
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
            this.buddy is not null &&
            rolePlayer &&
            rolePlayer.Data != null &&
            rolePlayer.Data.Tasks.Count != 0)
        {
            foreach (NetworkedPlayerInfo playerId in this.buddy.PlayerInfo)
            {
                if (Player.GetPlayerTaskGage(playerId) < this.awakeTaskGage)
                {
                    if (this.hiddeRole is IRoleAbility abilityRole)
                    {
                        abilityRole.Button.SetButtonShow(false);
                    }
                    return;
                }
            }
            this.awake = true;
            this.HasOtherVision = this.awakeHasOtherVision;
            if (this.hiddeRole != null)
            {
                this.AnotherRole = this.hiddeRole;
                this.CanHasAnotherRole = true;
                if (this.AnotherRole is IRoleAbility abilityRole)
                {
                    abilityRole.Button.SetButtonShow(true);
                }
                this.hiddeRole = null;
            }
        }
    }

    public override Color GetTargetRoleSeeColor(
        SingleRoleBase targetRole, byte targetPlayerId)
    {
        if (IsAwake &&
            this.buddy is not null &&
            this.buddy.Contains(targetPlayerId))
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
            return Design.ColoredString(
                Palette.White,
                Tr.GetString(RoleTypes.Crewmate.ToString()));
        }
    }

    public override string GetFullDescription()
    {
        if (IsAwake)
        {
            return string.Format(
                base.GetFullDescription(),
                this.buddy is null ?
                    string.Empty : this.buddy.GetAllPlayerName());
        }
        else
        {
            return Tr.GetString(
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
            return Design.ColoredString(
                Palette.White,
                $"{this.GetColoredRoleName()}: {Tr.GetString("crewImportantText")}");
        }
    }

    public override string GetIntroDescription()
    {
        if (IsAwake)
        {
            return string.Format(
                base.GetIntroDescription(),
                this.buddy is null ?
                    string.Empty : this.buddy.GetAllPlayerName());
        }
        else
        {
            return Design.ColoredString(
                Palette.CrewmateBlue,
                PlayerControl.LocalPlayer.Data.Role.Blurb);
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
            this.buddy is not null &&
            this.buddy.Contains(targetPlayerId))
        {
            return Design.ColoredString(
                ColorPalette.BuddyOrange,
                $" {GetRoleTag()}");
        }

        return base.GetRolePlayerNameTag(targetRole, targetPlayerId);
    }

    protected override void CreateSpecificOption(OptionCategoryScope<AutoParentSetBuilder> categoryScope)
    {
		categoryScope.Builder.CreateIntOption(
            BuddyOption.AwakeTaskGage,
            50, 0, 100, 10,
            format: OptionUnit.Percentage);
    }

    protected override void RoleSpecificInit()
    {
        this.awakeTaskGage = this.Loader.GetValue<BuddyOption, int>(
           BuddyOption.AwakeTaskGage) / 100.0f;

        this.awakeHasOtherVision = this.HasOtherVision;

        if (this.awakeTaskGage <= 0.0f)
        {
            this.awake = true;
            this.HasOtherVision = this.awakeHasOtherVision;
        }
        else
        {
            this.awake = false;
            this.HasOtherVision = false;
        }
    }

    private BuddyContainer getSameBuddy()
    {

        BuddyContainer buddy = new BuddyContainer();

        foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
        {
            var player = GameData.Instance.GetPlayerById(playerId);
            if (player != null &&
                this.IsSameControlId(role) &&
                (!player.IsDead || !player.Disconnected))
            {
                buddy.Add(player);
            }
        }

        return buddy;

    }
}

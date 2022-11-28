using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class BuddyManager : FlexibleCombinationRoleManagerBase
    {
        public BuddyManager() : base(new Buddy(), canAssignImposter: false)
        { }

    }

    public sealed class Buddy : MultiAssignRoleBase, IRoleAwake<RoleTypes>
    {
        public bool IsAwake => this.awake;

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum BuddyOption
        {
            
        }

        private bool awake;

        public Buddy() : base(
            ExtremeRoleId.Buddy,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Buddy.ToString(),
            ColorPalette.LoverPink,
            false, true,
            false, false,
            tab: OptionTab.Combination)
        { }

        public string GetFakeOptionString() => "";

        public void Update(PlayerControl rolePlayer)
        {
            throw new NotImplementedException();
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
                return Translation.GetString(
                    $"{this.Id}FullDescription");
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
                string baseString = base.GetIntroDescription();
                baseString += Design.ColoedString(
                    ColorPalette.LoverPink, "\n♥ ");

                List<byte> lover = getAliveSameLover();

                lover.Remove(CachedPlayerControl.LocalPlayer.PlayerId);

                byte firstLover = lover[0];
                lover.RemoveAt(0);

                baseString += Player.GetPlayerControlById(
                    firstLover).Data.PlayerName;
                if (lover.Count != 0)
                {
                    for (int i = 0; i < lover.Count; ++i)
                    {

                        if (i == 0)
                        {
                            baseString += Translation.GetString("andFirst");
                        }
                        else
                        {
                            baseString += Translation.GetString("and");
                        }
                        baseString += Player.GetPlayerControlById(
                            lover[i]).Data.PlayerName;

                    }
                }

                return string.Concat(
                    baseString,
                    Translation.GetString("LoverIntoPlus"),
                    Design.ColoedString(
                        ColorPalette.LoverPink, " ♥"));
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

        public override string GetRoleTag() => IsAwake ? "♥" : string.Empty;

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

        protected override void CreateSpecificOption(IOption parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new NotImplementedException();
        }

        private List<byte> getAliveSameLover()
        {

            List<byte> alive = new List<byte>();

            foreach (var item in ExtremeRoleManager.GameRole)
            {
                var player = GameData.Instance.GetPlayerById(item.Key);
                if (this.IsSameControlId(item.Value) &&
                    (!player.IsDead || !player.Disconnected))
                {
                    alive.Add(item.Key);
                }
            }

            return alive;

        }
    }
}

using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Survivor : SingleRoleBase, IRoleAwake<RoleTypes>, IRoleWinPlayerModifier
    {
        public override bool IsAssignGhostRole
        {
            get => this.isDeadWin || this.isNoWinSurvivorAssignGhostRole;
        }

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum SurvivorOption
        {
            AwakeTaskGage,
            DeadWinTaskGage,
            NoWinSurvivorAssignGhostRole
        }

        private bool awakeRole;
        private float awakeTaskGage;
        private bool awakeHasOtherVision;

        private bool isDeadWin;
        private float deadWinTaskGage;

        private bool isNoWinSurvivorAssignGhostRole;

        public Survivor() : base(
            ExtremeRoleId.Survivor,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Survivor.ToString(),
            ColorPalette.SurvivorYellow,
            false, true, false, false)
        { }

        public static void DeadWin(byte rolePlayerId)
        {
            Survivor survivor = ExtremeRoleManager.GetSafeCastedRole<Survivor>(rolePlayerId);
            if (survivor != null)
            {
                survivor.isDeadWin = true;
            }
        }

        public string GetFakeOptionString() => "";

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
            ref List<GameData.PlayerInfo> pulsWinner)
        {
            if (!rolePlayerInfo.IsDead || this.isDeadWin) { return; }

            switch (reason)
            {
                case GameOverReason.HumansByTask:
                case GameOverReason.HumansByVote:
                case GameOverReason.HumansDisconnect:
                    this.RemoveWinner(rolePlayerInfo, winner, pulsWinner);
                    break;
                default:
                    break;
            }
        }
        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awakeRole || !this.isDeadWin)
            {
                float taskGage = Player.GetPlayerTaskGage(rolePlayer);
                
                if (!this.isDeadWin && 
                    !rolePlayer.Data.IsDead &&
                    taskGage >= this.deadWinTaskGage)
                {
                    this.isDeadWin = true;
                    RPCOperator.Call(
                        rolePlayer.NetId,
                        RPCOperator.Command.SurvivorDeadWin,
                        new List<byte> { rolePlayer.PlayerId });
                    DeadWin(rolePlayer.PlayerId);
                }

                if (taskGage >= this.awakeTaskGage && !this.awakeRole)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
            }
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
                    Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
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
                return base.GetIntroDescription();
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

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

        public override void ExiledAction(
            GameData.PlayerInfo rolePlayer)
        {
            updateTaskDo();
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            updateTaskDo();
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                SurvivorOption.AwakeTaskGage,
                70, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                SurvivorOption.DeadWinTaskGage,
                100, 50, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateBoolOption(
                SurvivorOption.NoWinSurvivorAssignGhostRole,
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(SurvivorOption.AwakeTaskGage)].GetValue() / 100.0f;
            this.deadWinTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(SurvivorOption.DeadWinTaskGage)].GetValue() / 100.0f;
            this.isNoWinSurvivorAssignGhostRole = OptionHolder.AllOption[
                GetRoleOptionId(SurvivorOption.NoWinSurvivorAssignGhostRole)].GetValue();

            this.awakeHasOtherVision = this.HasOtherVison;
            this.isDeadWin = false;

            if (this.awakeTaskGage <= 0.0f)
            {
                this.awakeRole = true;
                this.HasOtherVison = this.awakeHasOtherVision;
            }
            else
            {
                this.awakeRole = false;
                this.HasOtherVison = false;
            }
        }

        private void updateTaskDo()
        {
            if (!this.isDeadWin)
            {
                this.HasTask = false;
            }
        }
    }
}

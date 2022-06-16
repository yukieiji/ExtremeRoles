using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class LastWolf : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
    {
        public enum LastWolfOption
        {
            AwakeImpostorNum,
            DeadPlayerNumBonus,
            KillPlayerNumBonus,
            FirstLightOffCoolTime
        }

        public RoleAbilityButtonBase Button
        {
            get => this.lightOffButton;
            set
            {
                this.lightOffButton = value;
            }
        }

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.isAwake;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

        private RoleAbilityButtonBase lightOffButton;

        private float finalCooltime;
        private float firstCooltime;

        private float noneAwakeKillBonus;
        private float deadPlayerKillBonus;

        private float defaultKillCool;

        private int noneAwakeKillCount;
        private int awakeImpNum;

        private bool isAwake;
        private bool isAwakedHasOtherVision;
        private bool isAwakedHasOtherKillCool;
        private bool isAwakedHasOtherKillRange;

        public LastWolf() : base(
            ExtremeRoleId.LastWolf,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.LastWolf.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public string GetFakeOptionString() => "";


        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("smash"),
                FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite);

            setCurCooltime();
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingStart()
        {
            setCurCooltime();
            if (this.isAwake)
            {
                this.HasOtherKillCool = true;
                float reduceRate = this.noneAwakeKillBonus * this.noneAwakeKillCount + 
                    this.deadPlayerKillBonus * (
                        GameData.Instance.AllPlayers.Count - computeAlivePlayerNum() - this.noneAwakeKillCount);
                this.KillCoolTime = this.KillCoolTime * (100.0f - reduceRate);
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }


        public bool UseAbility()
        {
            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.isAwake)
            {
                if (this.Button != null)
                {
                    this.Button.SetActive(false);
                }

                int impNum = 0;

                foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor())
                    {
                        ++impNum;
                    }
                }

                if (this.awakeImpNum >= impNum)
                {
                    this.isAwake = true;
                    this.HasOtherVison = this.isAwakedHasOtherVision;
                    this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                    this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
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
                    Palette.ImpostorRed, Translation.GetString(RoleTypes.Impostor.ToString()));
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
                    $"{RoleTypes.Impostor}FullDescription");
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
                return string.Concat(new string[]
                {
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                    "\r\n",
                    Palette.ImpostorRed.ToTextColor(),
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                    "</color>"
                });
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
                    Palette.ImpostorRed,
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
                return Palette.ImpostorRed;
            }
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (IsAwake)
            {
                this.KillCoolTime = this.defaultKillCool;
            }
            else
            {
                ++this.noneAwakeKillCount;
            }
            return true;
        }


        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateIntOption(
                LastWolfOption.AwakeImpostorNum,
                1, 1, OptionHolder.MaxImposterNum, 1,
                parentOps);

            CreateFloatOption(
                LastWolfOption.DeadPlayerNumBonus,
                1.0f, 2.0f, 6.5f, 0.1f,
                parentOps,
                format: OptionUnit.Percentage);

            CreateFloatOption(
                LastWolfOption.KillPlayerNumBonus,
                2.5f, 4.0f, 10.0f, 0.1f,
                parentOps,
                format: OptionUnit.Percentage);

            var firstLightOffOpt = CreateFloatDynamicOption(
                LastWolfOption.FirstLightOffCoolTime,
                5.0f, 0.5f, 0.5f,
                parentOps,
                format: OptionUnit.Second);

            this.CreateCommonAbilityOption(
                parentOps, 10f);

            var cooltimeOpt = OptionHolder.AllOption[
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)];
            int curSelection = cooltimeOpt.CurSelection;
            cooltimeOpt.CurSelection = Mathf.Clamp(
                curSelection * 2, 0, cooltimeOpt.Selections.Length - 1);
            cooltimeOpt.SetUpdateOption(firstLightOffOpt);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOpt = OptionHolder.AllOption;

            this.awakeImpNum = allOpt[
                GetRoleOptionId(LastWolfOption.AwakeImpostorNum)].GetValue();
            this.firstCooltime = allOpt[
                GetRoleOptionId(LastWolfOption.FirstLightOffCoolTime)].GetValue();
            this.finalCooltime = allOpt[
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue();

            this.noneAwakeKillBonus = allOpt[
                GetRoleOptionId(LastWolfOption.KillPlayerNumBonus)].GetValue();
            this.deadPlayerKillBonus = allOpt[
                GetRoleOptionId(LastWolfOption.DeadPlayerNumBonus)].GetValue();

            setCurCooltime();

            this.noneAwakeKillCount = 0;

            this.isAwakedHasOtherVision = false;
            this.isAwakedHasOtherKillCool = true;
            this.isAwakedHasOtherKillRange = false;

            if (this.HasOtherVison)
            {
                this.HasOtherVison = false;
                this.isAwakedHasOtherVision = true;
            }

            this.defaultKillCool = this.KillCoolTime;

            if (this.HasOtherKillCool)
            {
                this.HasOtherKillCool = false;
            }

            if (this.HasOtherKillRange)
            {
                this.HasOtherKillRange = false;
                this.isAwakedHasOtherKillRange = true;
            }

            if (this.awakeImpNum >= PlayerControl.GameOptions.NumImpostors)
            {
                this.isAwake = true;
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            }
        }

        private void setCurCooltime()
        {
            if (this.Button != null)
            {
                float curCool = (this.finalCooltime - this.firstCooltime) *
                    (1.0f - computeAlivePlayerNum() / GameData.Instance.AllPlayers.Count) + this.firstCooltime;
                this.Button.SetAbilityCoolTime(curCool);
            }
        }

        private int computeAlivePlayerNum()
        {
            var allPlayerList = GameData.Instance.AllPlayers;
            int alivePlayer = allPlayerList.Count;

            foreach (var player in allPlayerList.GetFastEnumerator())
            {
                if (player.IsDead || player.Disconnected)
                {
                    --alivePlayer;
                }
            }
            return alivePlayer;
        }

    }
}

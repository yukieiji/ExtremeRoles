using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class LastWolf : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
    {
        public static float LightOffVision { get; private set; } = 0.1f;

        public enum LastWolfOption
        {
            AwakeImpostorNum,
            DeadPlayerNumBonus,
            KillPlayerNumBonus,
            LightOffVision
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

        public static void SwitchLight(bool lightOn)
        {
            var vision = VisionComputer.Instance;

            if (lightOn)
            {
                vision.ResetModifier();
            }
            else
            {
                vision.SetModifier(
                   VisionComputer.Modifier.LastWolfLightOff);
            }
        }

        public string GetFakeOptionString() => "";

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("liightOff"),
                Resources.Loader.CreateSpriteFromResources(
                   Resources.Path.LastWolfLightOff),
                abilityCleanUp:CleanUp);
        }

        public bool IsAbilityUse() =>
            this.IsAwake &&
            this.IsCommonUse() &&
            VisionComputer.Instance.IsModifierResetted();

        public void RoleAbilityResetOnMeetingStart()
        {
            CleanUp();
            if (this.isAwake)
            {
                this.HasOtherKillCool = true;
                float reduceRate = this.noneAwakeKillBonus * this.noneAwakeKillCount + 
                    this.deadPlayerKillBonus * (float)(
                        GameData.Instance.AllPlayers.Count - computeAlivePlayerNum() - this.noneAwakeKillCount);
                this.KillCoolTime = this.KillCoolTime * (100.0f - reduceRate) / 100.0f;
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }


        public bool UseAbility()
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.LastWolfSwitchLight))
            {
                caller.WriteByte(byte.MaxValue);
            }
            SwitchLight(false);
            return true;
        }

        public void CleanUp()
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.LastWolfSwitchLight))
            {
                caller.WriteByte(byte.MinValue);
            }
            SwitchLight(true);
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
                    if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor() && 
                        (!player.IsDead && !player.Disconnected))
                    {
                        ++impNum;
                    }
                }

                if (this.awakeImpNum >= impNum)
                {
                    this.isAwake = true;
                    this.HasOtherVision = this.isAwakedHasOtherVision;
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
            IOption parentOps)
        {
            CreateIntOption(
                LastWolfOption.AwakeImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1,
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

            this.CreateCommonAbilityOption(
                parentOps, 10.0f);

            CreateFloatOption(
                LastWolfOption.LightOffVision,
                0.1f, 0.0f, 1.0f, 0.1f,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            var allOpt = OptionHolder.AllOption;

            this.awakeImpNum = allOpt[
                GetRoleOptionId(LastWolfOption.AwakeImpostorNum)].GetValue();

            this.noneAwakeKillBonus = allOpt[
                GetRoleOptionId(LastWolfOption.KillPlayerNumBonus)].GetValue();
            this.deadPlayerKillBonus = allOpt[
                GetRoleOptionId(LastWolfOption.DeadPlayerNumBonus)].GetValue();

            LightOffVision = allOpt[
                GetRoleOptionId(LastWolfOption.LightOffVision)].GetValue();

            this.noneAwakeKillCount = 0;

            this.isAwakedHasOtherVision = false;
            this.isAwakedHasOtherKillCool = true;
            this.isAwakedHasOtherKillRange = false;

            if (this.HasOtherVision)
            {
                this.HasOtherVision = false;
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

            if (this.awakeImpNum >= GameOptionsManager.Instance.CurrentGameOptions.GetInt(
                    Int32OptionNames.NumImpostors))
            {
                this.isAwake = true;
                this.HasOtherVision = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
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

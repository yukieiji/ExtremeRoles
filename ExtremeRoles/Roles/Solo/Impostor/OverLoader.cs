using System;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class OverLoader : SingleRoleBase, IRoleAbility, IRoleAwake<RoleTypes>
    {

        public enum OverLoaderOption
        {
            AwakeImpostorNum,
            AwakeKillCount,
            KillCoolReduceRate,
            MoveSpeed
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Impostor;

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.isAwake;
            }
        }

        public bool IsOverLoad;

        private float reduceRate;
        private float defaultKillCool;
        private int defaultKillRange;

        private bool isAwake;
        private int awakeImpNum;
        private int awakeKillCount;
        private int killCount;

        private bool isAwakedHasOtherVision;
        private bool isAwakedHasOtherKillCool;
        private bool isAwakedHasOtherKillRange;


        public RoleAbilityButtonBase Button
        {
            get => this.overLoadButton;
            set
            {
                this.overLoadButton = value;
            }
        }

        private RoleAbilityButtonBase overLoadButton;


        public OverLoader() : base(
            ExtremeRoleId.OverLoader,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.OverLoader.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {
            this.IsOverLoad = false;
        }
        
        public static void SwitchAbility(byte rolePlayerId, bool activate)
        {
            var overLoader = ExtremeRoleManager.GetSafeCastedRole<OverLoader>(rolePlayerId);
            if (overLoader != null)
            {
                overLoader.IsOverLoad = activate;
                overLoader.IsBoost = activate;
            }
        }

        public void CreateAbility()
        {
            this.CreatePassiveAbilityButton(
                Translation.GetString("overLoad"),
                Translation.GetString("downLoad"),
                Loader.CreateSpriteFromResources(
                   Path.OverLoaderOverLoad),
                Loader.CreateSpriteFromResources(
                   Path.OverLoaderDownLoad),
                this.CleanUp);
        }

        public bool IsAbilityUse() => 
            this.IsAwake && this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (IsAwake)
            {
                CleanUp();
            }
        }

        public bool UseAbility()
        {
            this.KillCoolTime = this.defaultKillCool * ((100f - this.reduceRate) / 100f);
            this.KillRange = 2;
            abilityOn();
            return true;
        }

        public void CleanUp()
        {
            this.KillCoolTime = this.defaultKillCool;
            this.KillRange = this.defaultKillRange;
            abilityOff();
        }

        public string GetFakeOptionString() => "";

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

                if (this.awakeImpNum >= impNum && 
                    this.killCount >= this.awakeKillCount)
                {
                    this.isAwake = true;
                    this.HasOtherVison = this.isAwakedHasOtherVision;
                    this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                    this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
                }
            }
        }

        public override bool TryRolePlayerKillTo(
            PlayerControl rolePlayer, PlayerControl targetPlayer)
        {
            if (!this.isAwake)
            {
                ++this.killCount;
            }
            return true;
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


        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                OverLoaderOption.AwakeImpostorNum,
                GameSystem.MaxImposterNum, 1,
                GameSystem.MaxImposterNum, 1,
                parentOps);

            CreateIntOption(
                OverLoaderOption.AwakeKillCount,
                0, 0, 3, 1,
                parentOps);

            this.CreateCommonAbilityOption(
                parentOps, 7.5f);

            CreateFloatOption(
                OverLoaderOption.KillCoolReduceRate,
                75.0f, 50.0f, 90.0f, 1.0f, parentOps,
                format: OptionUnit.Percentage);
            CreateFloatOption(
                OverLoaderOption.MoveSpeed,
                1.5f, 1.0f, 3.0f, 0.1f, parentOps,
                format: OptionUnit.Multiplier);
        }

        protected override void RoleSpecificInit()
        {
            var curOption = GameOptionsManager.Instance.CurrentGameOptions;

            if (!this.HasOtherKillCool)
            {
                this.HasOtherKillCool = true;
                this.KillCoolTime = curOption.GetFloat(FloatOptionNames.KillCooldown);
            }
            if (!this.HasOtherKillRange)
            {
                this.HasOtherKillRange = true;
                this.KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
            }

            this.defaultKillCool = this.KillCoolTime;
            this.defaultKillRange = this.KillRange;
            this.IsOverLoad = false;

            var allOption = OptionHolder.AllOption;

            this.awakeImpNum = allOption[
                GetRoleOptionId(OverLoaderOption.AwakeImpostorNum)].GetValue();
            this.awakeKillCount = allOption[
                GetRoleOptionId(OverLoaderOption.AwakeKillCount)].GetValue();

            this.MoveSpeed = allOption[
                GetRoleOptionId(OverLoaderOption.MoveSpeed)].GetValue();
            this.reduceRate = allOption[
                GetRoleOptionId(OverLoaderOption.KillCoolReduceRate)].GetValue();

            this.killCount = 0;

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

            if (this.awakeImpNum >= curOption.GetInt(Int32OptionNames.NumImpostors) && 
                this.awakeKillCount == 0)
            {
                this.isAwake = true;
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            }

            this.RoleAbilityInit();
        }

        private void abilityOn()
        {
            byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.OverLoaderSwitchAbility))
            {
                caller.WriteByte(localPlayerId);
                caller.WriteByte(byte.MaxValue);
            }
            SwitchAbility(localPlayerId, true);

        }
        private void abilityOff()
        {
            byte localPlayerId = CachedPlayerControl.LocalPlayer.PlayerId;

            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.OverLoaderSwitchAbility))
            {
                caller.WriteByte(localPlayerId);
                caller.WriteByte(byte.MinValue);
            }
            SwitchAbility(localPlayerId, false);
        }
    }
}

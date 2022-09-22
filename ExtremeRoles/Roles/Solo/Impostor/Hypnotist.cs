using System;
using System.Collections.Generic;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using ExtremeRoles.Compat.Interface;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Hypnotist : 
        SingleRoleBase,
        IRoleAbility,
        IRoleAwake<RoleTypes>,
        IRoleMurderPlayerHock,
        IRoleSpecialReset
    {
        public enum HypnotistOption
        {
            AwakeCheckImpostorNum,
            AwakeCheckTaskGage,
            AwakeKillCount,
            Range,
        }

        public enum RpcOps : byte
        {
            TargetToDoll,
            PickUpAbilityModule,
            ResetDollKillButton,
        }

        public enum AbilityModuleType : byte
        {
            Red,
            Blue,
            Glay
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

        private HashSet<byte> doll;

        private float dollKillCoolReduceRate;

        private bool isResetKillCoolWhenDollKill;

        private int awakeCheckImpNum;
        private float awakeCheckTaskGage;

        private bool isAwake;
        private bool canAwakeNow;
        private int killCount;
        private int awakeKillCount;

        private bool isAwakedHasOtherVision;
        private bool isAwakedHasOtherKillCool;
        private bool isAwakedHasOtherKillRange;

        private float defaultKillCool;
        private float range;

        private PlayerControl target;

        public Hypnotist() : base(
            ExtremeRoleId.Hypnotist,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Hypnotist.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public static void Ability(ref MessageReader reader)
        {
            byte rolePlayerId = reader.ReadByte();
            Hypnotist role = ExtremeRoleManager.GetSafeCastedRole<Hypnotist>(rolePlayerId);
            RpcOps ops = (RpcOps)reader.ReadByte();
            switch (ops)
            {
                case RpcOps.TargetToDoll:
                    byte targetPlayerId = reader.ReadByte();
                    targetToDoll(role, rolePlayerId, targetPlayerId);
                    break;
                case RpcOps.PickUpAbilityModule:
                    updateDoll(role, ref reader);
                    break;
                case RpcOps.ResetDollKillButton:
                    resetDollKillButton(role);
                    break;
            }
        }

        public static void UpdateAllDollKillButtonState(Hypnotist role)
        {
            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            float optionKillCool = PlayerControl.GameOptions.KillCooldown;
            foreach (byte dollPlayerId in role.doll)
            {
                SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
                if (doll.Id == ExtremeRoleId.Doll)
                {
                    float curKillCool = localPlayer.killTimer;
                    if (localPlayer.PlayerId == dollPlayerId &&
                        doll.CanKill &&
                        curKillCool > 0.0f)
                    {
                        localPlayer.killTimer = Mathf.Clamp(
                            curKillCool * role.dollKillCoolReduceRate,
                            0.001f, optionKillCool);
                    }
                    doll.CanKill = true;
                }
            }
        }

        public static void FeatAllDollMapModuleAccess(
            Hypnotist role, SystemConsoleType console)
        {
            foreach (byte dollPlayerId in role.doll)
            {
                SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
                if (doll is Doll castedDoll)
                {
                    castedDoll.FeatMapModuleAccess(console);
                }
            }
        }

        public static void UnlockAllDollCrakingAbility(
            Hypnotist role, SystemConsoleType unlockConsole)
        {
            foreach (byte dollPlayerId in role.doll)
            {
                SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
                if (doll is Doll castedDoll)
                {
                    castedDoll.UnlockCrakingAbility(unlockConsole);
                }
            }
        }

        private static void targetToDoll(
            Hypnotist role,
            byte rolePlayerId,
            byte targetPlayerId)
        {
            // TODO : ドールの初期化処理



            if (rolePlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                setAbilityPart();
            }
        }

        private static void updateDoll(
            Hypnotist role,
            ref MessageReader reader)
        {
            AbilityModuleType type = (AbilityModuleType)reader.ReadByte();
            switch (type)
            {
                case AbilityModuleType.Red:
                    UpdateAllDollKillButtonState(role);
                    break;
                case AbilityModuleType.Blue:
                    SystemConsoleType featAbilityConsole = (SystemConsoleType)reader.ReadByte();
                    FeatAllDollMapModuleAccess(role, featAbilityConsole);
                    break;
                case AbilityModuleType.Glay:
                    SystemConsoleType unlockConsole = (SystemConsoleType)reader.ReadByte();
                    UnlockAllDollCrakingAbility(role, unlockConsole);
                    break;
                default:
                    break;
            }
        }

        private static void resetDollKillButton(Hypnotist role)
        {
            foreach (byte dollPlayerId in role.doll)
            {
                SingleRoleBase doll = ExtremeRoleManager.GameRole[dollPlayerId];
                if (doll.Id == ExtremeRoleId.Doll)
                {
                    doll.CanKill = false;
                }
            }
        }

        public string GetFakeOptionString() => "";

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("liightOff"),
                Resources.Loader.CreateSpriteFromResources(
                   Resources.Path.LastWolfLightOff));
        }

        public bool IsAbilityUse()
        {
            this.target = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer,
                this, this.range);

            return this.target != null && this.IsCommonUse();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.isAwake && this.doll.Count > 0)
            {

                PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;

                RPCOperator.Call(
                   rolePlayer.NetId,
                   RPCOperator.Command.HypnotistAbility,
                   new List<byte>
                   {
                        (byte)RpcOps.ResetDollKillButton,
                   });
                resetDollKillButton(this);
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            if (this.killCount >= this.awakeKillCount)
            {
                this.isAwake = true;
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            }
        }

        public bool UseAbility()
        {
            PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;
            byte targetPlayerId = this.target.PlayerId;


            RPCOperator.Call(
                rolePlayer.NetId,
               RPCOperator.Command.HypnotistAbility,
               new List<byte>
               {
                    (byte)RpcOps.TargetToDoll,
                    targetPlayerId,
               });
            targetToDoll(this, rolePlayer.PlayerId, targetPlayerId);
            this.target = null;

            return true;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.canAwakeNow)
            {
                int impNum = 0;

                foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
                {
                    if (ExtremeRoleManager.GameRole[player.PlayerId].IsImpostor() &&
                        (!player.IsDead && !player.Disconnected))
                    {
                        ++impNum;
                    }
                }

                GameData gameData = GameData.Instance;

                if (this.awakeCheckImpNum >= impNum ||
                    this.awakeCheckTaskGage >= (gameData.CompletedTasks / gameData.TotalTasks))
                {
                    this.canAwakeNow = true;
                    this.killCount = 0;
                }
            }
            if (!this.isAwake)
            {
                if (this.Button != null)
                {
                    this.Button.SetActive(false);
                }
            }
        }

        public void HockMuderPlayer(
            PlayerControl source, PlayerControl target)
        {
            if (this.doll.Contains(source.PlayerId) &&
                this.isResetKillCoolWhenDollKill)
            {
                CachedPlayerControl.LocalPlayer.PlayerControl.killTimer = this.defaultKillCool;
            }
        }

        public void AllReset(PlayerControl rolePlayer)
        {
            foreach (byte playerId in this.doll)
            {
                PlayerControl player = Player.GetPlayerControlById(playerId);

                if (player == null) { continue; }

                if (player.Data.IsDead ||
                    player.Data.Disconnected) { continue; }

                RPCOperator.UncheckedMurderPlayer(
                    playerId, playerId,
                    byte.MaxValue);
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
            if (this.canAwakeNow && !this.isAwake)
            {
                ++this.killCount;
            }
            return true;
        }

        public override void ExiledAction(GameData.PlayerInfo rolePlayer)
        {
            foreach (byte playerId in this.doll)
            {
                PlayerControl player = Player.GetPlayerControlById(playerId);

                if (player == null) { continue; }
                if (player.Data.IsDead || player.Data.Disconnected) { continue; }

                player.Exiled();
            }
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer, PlayerControl killerPlayer)
        {
            foreach (byte playerId in this.doll)
            {
                PlayerControl player = Player.GetPlayerControlById(playerId);

                if (player == null) { continue; }

                if (player.Data.IsDead ||
                    player.Data.Disconnected) { continue; }

                RPCOperator.UncheckedMurderPlayer(
                    playerId, playerId,
                    byte.MaxValue);
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                HypnotistOption.AwakeCheckImpostorNum,
                1, 1, OptionHolder.MaxImposterNum, 1,
                parentOps);
            CreateIntOption(
                HypnotistOption.AwakeCheckTaskGage,
                60, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                HypnotistOption.AwakeKillCount,
                2, 0, 5, 1, parentOps,
                format: OptionUnit.Shot);

            this.CreateAbilityCountOption(parentOps, 1, 5);

            CreateFloatOption(
                HypnotistOption.Range,
                1.0f, 0.5f, 2.6f, 0.1f,
                parentOps);
        }

        protected override void RoleSpecificInit()
        {
            this.RoleAbilityInit();

            this.defaultKillCool = PlayerControl.GameOptions.KillCooldown;

            if (this.HasOtherKillCool)
            {
                this.defaultKillCool = this.KillCoolTime;
            }

            var allOpt = OptionHolder.AllOption;
            this.awakeCheckImpNum = allOpt[
                GetRoleOptionId(HypnotistOption.AwakeCheckImpostorNum)].GetValue();
            this.awakeCheckTaskGage = (float)allOpt[
                GetRoleOptionId(HypnotistOption.AwakeCheckTaskGage)].GetValue() / 100.0f;
            this.awakeKillCount = allOpt[
                GetRoleOptionId(HypnotistOption.AwakeKillCount)].GetValue();

            this.range = allOpt[
                GetRoleOptionId(HypnotistOption.Range)].GetValue();

            this.canAwakeNow =
                this.awakeCheckImpNum >= PlayerControl.GameOptions.NumImpostors &&
                this.awakeCheckTaskGage <= 0.0f;

            this.killCount = 0;

            if (this.canAwakeNow && this.awakeKillCount <= 0)
            {
                this.isAwake = true;
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            }
        }

        private static void setAbilityPart()
        {
            // 能力のかけらの設置処理
        }
    }

    public sealed class Doll : SingleRoleBase, IRoleAbility, IRoleUpdate, IRoleHasParent
    {
        public RoleAbilityButtonBase Button
        { 
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public byte Parent => throw new NotImplementedException();

        public enum LastWolfOption
        {
            AwakeImpostorNum,
            DeadPlayerNumBonus,
            KillPlayerNumBonus,
            FinalLightOffCoolTime
        }
        public Doll() : base(
            ExtremeRoleId.Doll,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Doll.ToString(),
            Palette.ImpostorRed,
            false, false, false,
            false, false, false,
            false, false, false)
        { }

        public void FeatMapModuleAccess(SystemConsoleType consoleType)
        {
            switch (consoleType)
            {
                case SystemConsoleType.Admin:
                    this.CanUseAdmin = true;
                    break;
                case SystemConsoleType.SecurityCamera:
                    this.CanUseSecurity = true;
                    break;
                case SystemConsoleType.Vital:
                    this.CanUseVital = true;
                    break;
                case SystemConsoleType.EmergencyButton:
                    this.CanCallMeeting = true;
                    break;
                default:
                    break;
            }
        }

        public void UnlockCrakingAbility(SystemConsoleType consoleType)
        {
            switch (consoleType)
            {
                case SystemConsoleType.Admin:
                    this.CanUseAdmin = true;
                    break;
                case SystemConsoleType.SecurityCamera:
                    
                    break;
                case SystemConsoleType.Vital:
                    
                    break;
                default:
                    break;
            }
        }


        public void CreateAbility()
        {
            throw new NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new NotImplementedException();
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new NotImplementedException();
        }

        public void RemoveParent(byte rolePlayerId)
        {
            throw new NotImplementedException();
        }
    }
}

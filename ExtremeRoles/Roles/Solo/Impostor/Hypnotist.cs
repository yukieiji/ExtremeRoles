using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using Hazel;

using Newtonsoft.Json.Linq;

using BepInEx.IL2CPP.Utils.Collections;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.Mods;

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
            HideArrowRange,
            DefaultRedAbilityPart,
            HideKillButtonTime,
            IsResetKillCoolWhenDollKill,
            DollKillCoolReduceRate,
            DollCrakingCoolTime,
            DollCrakingActiveTime
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
        private int defaultRedAbilityPartNum;

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

        private JObject position;
        private const string postionJson = "ExtremeRoles.Resources.Position.Hypnotist.json";

        private const string adminKey = "Admin";
        private const string securityKey = "Security";
        private const string vitalKey = "Vital";

        private const string skeldKey = "Skeld";
        private const string miraHqKey = "MiraHQ";
        private const string polusKey = "Polus";
        private const string airShipKey = "AirShip";

        private List<Vector3> addedPos;
        private List<Vector3> addRedPos;
        private int addRedPosNum;

        private float hideDistance = 7.5f;

        public float DollCrakingCoolTime => this.dollCrakingCoolTime;
        public float DollCrakingActiveTime => this.dollCrakingActiveTime;

        private float dollCrakingCoolTime;
        private float dollCrakingActiveTime;

        private bool isActiveTimer;
        private float timer;
        private float defaultTimer;

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
            Logging.Debug($"FeatAccess:{console}");
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
            Logging.Debug($"unlock:{unlockConsole}");
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
            IRoleSpecialReset.ResetRole(targetPlayerId);
            Doll newDoll = new Doll(targetPlayerId, rolePlayerId, role);
            if (targetPlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
            {
                newDoll.CreateAbility();
            }
            ExtremeRoleManager.SetNewRole(targetPlayerId, newDoll);
            role.doll.Add(targetPlayerId);
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

        public void EnableKillTimer()
        {
            this.timer = this.defaultTimer;
            this.isActiveTimer = true;
        }

        public void RemoveDoll(byte playerId)
        {
            this.doll.Remove(playerId);
        }

        public void RemoveAbilityPartPos(Vector3 pos)
        {
            this.addedPos.Remove(pos);
        }

        public string GetFakeOptionString() => "";

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("liightOff"),
                Resources.Loader.CreateSpriteFromResources(
                   Resources.Path.LastWolfLightOff));

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                postionJson);
            var byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);
            this.position = JObject.Parse(
                Encoding.UTF8.GetString(byteArray));
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
                        rolePlayer.PlayerId,
                        (byte)RpcOps.ResetDollKillButton,
                   });
                resetDollKillButton(this);
            }
            this.isActiveTimer = false;
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
            if (this.isAwake && this.addRedPos.Count > 0)
            {
                setRedAbilityPart(this.addRedPos.Count);
            }
        }

        public bool UseAbility()
        {
            PlayerControl rolePlayer = CachedPlayerControl.LocalPlayer;
            byte targetPlayerId = this.target.PlayerId;

            SingleRoleBase role = ExtremeRoleManager.GameRole[targetPlayerId];
            MultiAssignRoleBase multiAssignRole = role as MultiAssignRoleBase;

            int redPartNum = this.defaultRedAbilityPartNum;
            Type roleType = role.GetType();
            Type[] interfaces = roleType.GetInterfaces();

            redPartNum += computeRedPartNum(interfaces);

            if (multiAssignRole != null)
            {
                if (multiAssignRole.AnotherRole != null)
                {
                    Type anotherRoleType = multiAssignRole.AnotherRole.GetType();
                    Type[] anotherInterface = anotherRoleType.GetInterfaces();
                    redPartNum += computeRedPartNum(anotherInterface);
                }
            }


            RPCOperator.Call(
                rolePlayer.NetId,
                RPCOperator.Command.HypnotistAbility,
                new List<byte>
                {
                    rolePlayer.PlayerId,
                    (byte)RpcOps.TargetToDoll,
                    targetPlayerId,
                });
            targetToDoll(this, rolePlayer.PlayerId, targetPlayerId);
            setAbilityPart(redPartNum);
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

            if (this.isActiveTimer)
            {
                this.timer -= Time.fixedDeltaTime;
                if (this.timer <= 0.0f)
                {
                    Logging.Debug("ResetKillButton");
                    this.isActiveTimer = false;
                    RPCOperator.Call(
                       rolePlayer.NetId,
                       RPCOperator.Command.HypnotistAbility,
                       new List<byte>
                       {
                           rolePlayer.PlayerId,
                           (byte)RpcOps.ResetDollKillButton,
                       });
                    resetDollKillButton(this);
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

            CreateFloatOption(
                HypnotistOption.Range,
                1.6f, 0.5f, 5.0f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(parentOps, 1, 5);

            CreateFloatOption(
                HypnotistOption.HideArrowRange,
                10.0f, 5.0f, 25.0f, 0.5f,
                parentOps);
            CreateIntOption(
                HypnotistOption.DefaultRedAbilityPart,
                0, 0, 10, 1,
                parentOps);
            CreateFloatOption(
                HypnotistOption.HideKillButtonTime,
                15.0f, 2.5f, 60.0f, 0.5f,
                parentOps,
                format: OptionUnit.Second);
            CreateBoolOption(
                HypnotistOption.IsResetKillCoolWhenDollKill,
                true, parentOps);
            CreateIntOption(
                HypnotistOption.DollKillCoolReduceRate,
                10, 0, 75, 5,
                parentOps,
                format: OptionUnit.Percentage);
            CreateFloatOption(
                HypnotistOption.DollCrakingCoolTime,
                30.0f, 0.5f, 120.0f, 0.5f,
                parentOps,
                format: OptionUnit.Second);
            CreateFloatOption(
                HypnotistOption.DollCrakingActiveTime,
                3.0f, 0.5f, 60.0f, 0.5f,
                parentOps,
                format: OptionUnit.Second);

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

            this.hideDistance = allOpt[
                GetRoleOptionId(HypnotistOption.HideArrowRange)].GetValue();
            this.isResetKillCoolWhenDollKill = allOpt[
                GetRoleOptionId(HypnotistOption.IsResetKillCoolWhenDollKill)].GetValue();
            this.dollKillCoolReduceRate = ((1.0f - (float)allOpt[
                GetRoleOptionId(HypnotistOption.DollKillCoolReduceRate)].GetValue()) / 100.0f);
            this.defaultRedAbilityPartNum = allOpt[
                GetRoleOptionId(HypnotistOption.DefaultRedAbilityPart)].GetValue();

            this.dollCrakingActiveTime = allOpt[
                GetRoleOptionId(HypnotistOption.DollCrakingActiveTime)].GetValue();
            this.dollCrakingCoolTime = allOpt[
                GetRoleOptionId(HypnotistOption.DollCrakingCoolTime)].GetValue();

            this.defaultTimer = allOpt[
                GetRoleOptionId(HypnotistOption.HideKillButtonTime)].GetValue();

            this.canAwakeNow =
                this.awakeCheckImpNum >= PlayerControl.GameOptions.NumImpostors &&
                this.awakeCheckTaskGage <= 0.0f;

            this.killCount = 0;

            this.doll = new HashSet<byte>();

            this.isAwakedHasOtherVision = false;
            this.isAwakedHasOtherKillCool = true;
            this.isAwakedHasOtherKillRange = false;

            if (this.HasOtherVison)
            {
                this.HasOtherVison = false;
                this.isAwakedHasOtherVision = true;
            }

            if (this.HasOtherKillCool)
            {
                this.HasOtherKillCool = false;
            }

            if (this.HasOtherKillRange)
            {
                this.HasOtherKillRange = false;
                this.isAwakedHasOtherKillRange = true;
            }

            if (this.canAwakeNow && this.awakeKillCount <= 0)
            {
                this.isAwake = true;
                this.HasOtherVison = this.isAwakedHasOtherVision;
                this.HasOtherKillCool = this.isAwakedHasOtherKillCool;
                this.HasOtherKillRange = this.isAwakedHasOtherKillRange;
            }
            this.doll = new HashSet<byte>();
            this.addedPos = new List<Vector3>();
            this.addRedPos = new List<Vector3>();
            this.addRedPosNum = 0;

            this.isActiveTimer = false;
        }

        private void setAbilityPart(int redModuleNum)
        {
            byte mapId = PlayerControl.GameOptions.MapId;

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                if (ExtremeRolesPlugin.Compat.ModMap is SubmergedMap)
                {
                    setAbilityPartFromMapJsonInfo(
                        this.position["Submerged"], redModuleNum);
                }
            }
            else
            { 
                switch (mapId)
                {
                    case 0:
                        setAbilityPartFromMapJsonInfo(
                            this.position[skeldKey], redModuleNum);
                        break;
                    case 1:
                        setAbilityPartFromMapJsonInfo(
                            this.position[miraHqKey], redModuleNum);
                        break;
                    case 2:
                        setAbilityPartFromMapJsonInfo(
                            this.position[polusKey], redModuleNum);
                        break;
                    case 4:
                        setAbilityPartFromMapJsonInfo(
                            this.position[airShipKey], redModuleNum);
                        break;
                    default:
                        break;
                }
            }
        }
        private void setAbilityPartFromMapJsonInfo(
            JToken json, int redNum)
        {
            JArray jsonRedPos = json["Red"].TryCast<JArray>();
            
            List<Vector3> redPos = new List<Vector3>();
            for (int i = 0; i < jsonRedPos.Count; ++i)
            {
               JArray pos = jsonRedPos[i].TryCast<JArray>();
               redPos.Add(new Vector3((float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f)));
            }

            this.addRedPosNum = jsonRedPos.Count;

            JToken adminPos;
            JToken securiPos;
            JToken vitalPos;

            List<(Vector3, SystemConsoleType)> bluePos = new List<(Vector3, SystemConsoleType)>();
            JObject jsonBluePos = json["Blue"].TryCast<JObject>();

            if (jsonBluePos.TryGetValue(adminKey, out adminPos))
            {
                JArray pos = adminPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                    (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    bluePos.Add((vecPos, SystemConsoleType.Admin));
                }
            }
            if (jsonBluePos.TryGetValue(securityKey, out securiPos))
            {
                JArray pos = securiPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                    (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    bluePos.Add((vecPos, SystemConsoleType.SecurityCamera));
                }
            }
            if (jsonBluePos.TryGetValue(vitalKey, out vitalPos))
            {
                JArray pos = vitalPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                    (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    bluePos.Add((vecPos, SystemConsoleType.Vital));
                }
            }

            List<(Vector3, SystemConsoleType)> grayPos = new List<(Vector3, SystemConsoleType)>();
            JObject jsonGrayPos = json["Gray"].TryCast<JObject>();

            if (jsonGrayPos.TryGetValue(adminKey, out adminPos))
            {
                JArray pos = adminPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                    (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    grayPos.Add((vecPos, SystemConsoleType.Admin));
                }
            }
            if (jsonGrayPos.TryGetValue(securityKey, out securiPos))
            {
                JArray pos = securiPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                    (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    grayPos.Add((vecPos, SystemConsoleType.SecurityCamera));
                }
            }
            if (jsonGrayPos.TryGetValue(vitalKey, out vitalPos))
            {
                JArray pos = vitalPos.TryCast<JArray>();
                Vector3 vecPos = new Vector3(
                     (float)pos[0], (float)pos[1], (((float)pos[1]) / 1000.0f));
                if (!this.addedPos.Contains(vecPos))
                {
                    grayPos.Add((vecPos, SystemConsoleType.Vital));
                }
            }

            List<Vector3> noneSortedAddPos = new List<Vector3>();

            for (int i = 0; i < redNum; ++i)
            {
                int useIndex = i % redPos.Count;
                noneSortedAddPos.Add(redPos[useIndex]);
            }

            this.addRedPos = noneSortedAddPos.OrderBy(
                x => RandomGenerator.Instance.Next()).ToList();
            setRedAbilityPart(redNum);

            foreach (var (pos, console) in grayPos)
            {
                GameObject obj = new GameObject("GrayAbilityPart");
                obj.transform.position = pos;
                GrayAbilityPart grayAbilityPart = obj.AddComponent<GrayAbilityPart>();
                grayAbilityPart.SetHideArrowDistance(this.hideDistance);
                grayAbilityPart.SetConsoleType(console);
            }
            foreach (var (pos, console) in bluePos)
            {
                GameObject obj = new GameObject("BlueAbilityPart");
                obj.transform.position = pos;
                BlueAbilityPart blueAbilityPart = obj.AddComponent<BlueAbilityPart>();
                blueAbilityPart.SetHideArrowDistance(this.hideDistance);
                blueAbilityPart.SetConsoleType(console);
            }
        }
        private void setRedAbilityPart(int maxSetNum)
        {
            int setNum = Math.Min(this.addRedPosNum, maxSetNum);
            int checkIndex = 0;
            for (int i = 0; i < setNum; ++i)
            {
                Vector3 pos = this.addRedPos[checkIndex];

                if (this.addedPos.Contains(pos))
                {
                    ++checkIndex;
                    continue; 
                }

                GameObject obj = new GameObject("RedAbilityPart");
                obj.transform.position = pos;
                RedAbilityPart redAbilityPart = obj.AddComponent<RedAbilityPart>();
                redAbilityPart.SetHideArrowDistance(this.hideDistance);
                this.addRedPos.RemoveAt(checkIndex);
                this.addedPos.Add(pos);
            }
        }
        private static int computeRedPartNum(Type[] interfaces)
        {
            int num = 0;

            foreach (Type @interface in interfaces)
            {
                int addNum;
                string name = @interface.FullName;
                name = name.Replace("ExtremeRoles.Roles.API.Interface.","");
                switch (name)
                {
                    case "IRoleVoteModifier":
                        addNum = 9;
                        break;
                    case "IRoleMeetingButtonAbility":
                        addNum = 8;
                        break;
                    case "IRoleAwake":
                        addNum = 7;
                        break;
                    case "IRoleOnRevive":
                        addNum = 6;
                        break;
                    case "IRoleAbility":
                        addNum = 5;
                        break;
                    case "IRoleMurderPlayerHock":
                        addNum = 4;
                        break;
                    case "IRoleUpdate":
                        addNum = 3;
                        break;
                    case "IRoleReportHock":
                        addNum = 2;
                        break;
                    default:
                        addNum = 1;
                        break;
                }
                num += addNum;
            }

            return num;
        }
    }

    public sealed class Doll : 
        SingleRoleBase,
        IRoleAbility,
        IRoleUpdate,
        IRoleHasParent,
        IRoleWinPlayerModifier
    {
        public enum AbilityType : byte
        {
            Admin,
            Security,
            Vital,
        }

        public RoleAbilityButtonBase Button
        { 
            get => this.crakingButton;
            set
            {
                this.crakingButton = value;
            }
        }

        public byte Parent => this.hypnotistPlayerId;

        private byte hypnotistPlayerId;
        private Hypnotist hypnotist;
        private byte dollPlayerId;

        private AbilityType curAbilityType;
        private AbilityType nextUseAbilityType;
        private TMPro.TextMeshPro chargeTime;

        private Sprite adminSprite;
        private Sprite securitySprite;
        private Sprite vitalSprite;

        private Minigame minigame;
        private HashSet<AbilityType> canUseCrakingModule;

        private RoleAbilityButtonBase crakingButton;

        private TMPro.TextMeshPro tellText;

        private bool prevKillState;

        public Doll(
            byte dollPlayerId,
            byte hypnotistPlayerId,
            Hypnotist parent) : base(
            ExtremeRoleId.Doll,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Doll.ToString(),
            Palette.ImpostorRed,
            false, false, false,
            false, false, false,
            false, false, false)
        {
            this.dollPlayerId = dollPlayerId;
            this.hypnotistPlayerId = hypnotistPlayerId;
            this.hypnotist = parent;
            this.FakeImposter = true;
            this.canUseCrakingModule = new HashSet<AbilityType>();
            this.prevKillState = false;
        }

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
                default:
                    break;
            }
            if (CachedPlayerControl.LocalPlayer.PlayerId == this.dollPlayerId)
            {
                showText(string.Format(
                    Translation.GetString("FeatAccess"),
                    Translation.GetString(consoleType.ToString())));
            }
        }

        public void UnlockCrakingAbility(SystemConsoleType consoleType)
        {
            AbilityType addType;
            switch (consoleType)
            {
                case SystemConsoleType.Admin:
                    addType = AbilityType.Admin;
                    break;
                case SystemConsoleType.SecurityCamera:
                    addType = AbilityType.Security;
                    break;
                case SystemConsoleType.Vital:
                    addType = AbilityType.Vital;
                    break;
                default:
                    return;
            }
            if (this.canUseCrakingModule.Count == 0)
            {
                this.nextUseAbilityType = addType;
            }
            this.canUseCrakingModule.Add(addType);
            
            if (CachedPlayerControl.LocalPlayer.PlayerId == this.dollPlayerId)
            {
                showText(string.Format(
                    Translation.GetString("unlockCrakking"),
                    Translation.GetString(consoleType.ToString())));
            }
        }

        public void RemoveParent(byte rolePlayerId)
        {
            this.hypnotist.RemoveDoll(rolePlayerId);
        }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            ref Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner,
            ref List<GameData.PlayerInfo> pulsWinner)
        {
            switch (reason)
            {
                case GameOverReason.ImpostorByVote:
                case GameOverReason.ImpostorByKill:
                case GameOverReason.ImpostorBySabotage:
                case GameOverReason.ImpostorDisconnect:
                case (GameOverReason)RoleGameOverReason.AssassinationMarin:
                    this.AddWinner(rolePlayerInfo, winner, pulsWinner);
                    break;
                default:
                    break;
            }
        }

        public void CreateAbility()
        {
            this.adminSprite = GameSystem.GetAdminButtonImage();
            this.securitySprite = GameSystem.GetSecurityImage();
            this.vitalSprite = GameSystem.GetVitalImage();

            this.Button = new ChargableButton(
                Translation.GetString("traitorCracking"),
                UseAbility,
                IsAbilityUse,
                this.adminSprite,
                new Vector3(-1.8f, -0.06f, 0),
                CleanUp,
                CheckAbility,
                KeyCode.F,
                false);

            this.Button.SetAbilityCoolTime(
                hypnotist.DollCrakingCoolTime);
            this.Button.SetAbilityActiveTime(
                hypnotist.DollCrakingActiveTime);
        }

        public bool UseAbility()
        {
            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    FastDestroyableSingleton<HudManager>.Instance.ShowMap(
                        (Action<MapBehaviour>)(m => m.ShowCountOverlay()));
                    break;
                case AbilityType.Security:
                    SystemConsole watchConsole = GameSystem.GetSecuritySystemConsole();
                    if (watchConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = ChargableButton.OpenMinigame(
                        watchConsole.MinigamePrefab);
                    break;
                case AbilityType.Vital:
                    SystemConsole vitalConsole = GameSystem.GetVitalSystemConsole();
                    if (vitalConsole == null || Camera.main == null)
                    {
                        return false;
                    }
                    this.minigame = ChargableButton.OpenMinigame(
                        vitalConsole.MinigamePrefab);
                    break;
                default:
                    return false;
            }

            this.curAbilityType = this.nextUseAbilityType;

            updateAbility();
            updateButtonSprite();

            return true;
        }

        public bool CheckAbility()
        {
            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    return MapBehaviour.Instance.isActiveAndEnabled;
                case AbilityType.Security:
                case AbilityType.Vital:
                    return Minigame.Instance != null;
                default:
                    return false;
            }
        }

        public void CleanUp()
        {
            switch (this.curAbilityType)
            {
                case AbilityType.Admin:
                    if (MapBehaviour.Instance)
                    {
                        MapBehaviour.Instance.Close();
                    }
                    break;
                case AbilityType.Security:
                case AbilityType.Vital:
                    if (this.minigame != null)
                    {
                        this.minigame.Close();
                        this.minigame = null;
                    }
                    break;
                default:
                    break;
            }
        }

        public bool IsAbilityUse()
        {

            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    return
                        this.IsCommonUse() &&
                        (
                            MapBehaviour.Instance == null ||
                            !MapBehaviour.Instance.isActiveAndEnabled
                        );
                case AbilityType.Security:
                case AbilityType.Vital:
                    return this.IsCommonUse() && Minigame.Instance == null;
                default:
                    return false;
            }
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            if (this.chargeTime != null)
            {
                this.chargeTime.gameObject.SetActive(false);
            }
            if (this.minigame != null)
            {
                this.minigame.Close();
                this.minigame = null;
            }
            if (MapBehaviour.Instance)
            {
                MapBehaviour.Instance.Close();
            }
            if (this.tellText != null)
            {
                this.tellText.gameObject.SetActive(false);
            }
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void Update(PlayerControl rolePlayer)
        {
            PlayerControl hypnotistPlayer = Player.GetPlayerControlById(this.hypnotistPlayerId);
            if (!rolePlayer.Data.IsDead &&
                (hypnotistPlayer == null || hypnotistPlayer.Data.IsDead))
            {
                RPCOperator.Call(
                    rolePlayer.NetId,
                    RPCOperator.Command.UncheckedMurderPlayer,
                    new List<byte> { rolePlayer.PlayerId, rolePlayer.PlayerId, 0 });
                RPCOperator.UncheckedMurderPlayer(
                    rolePlayer.PlayerId,
                    rolePlayer.PlayerId, 0);
            }

            if (this.canUseCrakingModule.Count == 0 && 
                this.Button != null)
            {
                this.Button.SetActive(false);
            }

            if (MeetingHud.Instance == null &&
                this.prevKillState != this.CanKill)
            {
                showText(
                    this.CanKill ?
                    Translation.GetString("unlockKill") :
                    Translation.GetString("lockKill"));
            }

            this.prevKillState = this.CanKill;

            if (this.chargeTime == null)
            {
                this.chargeTime = UnityEngine.Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                    Camera.main.transform, false);
                this.chargeTime.transform.localPosition = new Vector3(3.5f, 2.25f, -250.0f);
            }

            if (!this.Button.IsAbilityActive())
            {
                this.chargeTime.gameObject.SetActive(false);
                return;
            }

            this.chargeTime.text = Mathf.CeilToInt(this.Button.GetCurTime()).ToString();
            this.chargeTime.gameObject.SetActive(true);
        }

        public override Color GetTargetRoleSeeColor(
            SingleRoleBase targetRole,
            byte targetPlayerId)
        {

            if (targetPlayerId == this.hypnotistPlayerId)
            {
                return Palette.ImpostorRed;
            }
            return base.GetTargetRoleSeeColor(targetRole, targetPlayerId);
        }

        public override string GetFullDescription()
        {
            return string.Format(
                base.GetFullDescription(),
                Player.GetPlayerControlById(
                    this.hypnotistPlayerId)?.Data.PlayerName);
        }

        public override bool IsSameTeam(SingleRoleBase targetRole)
        {
            if (targetRole.Id == this.Id)
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
                return targetRole.IsImpostor();
            }
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            throw new Exception("Don't call this class method!!");
        }

        protected override void RoleSpecificInit()
        {
            throw new Exception("Don't call this class method!!");
        }

        private void updateAbility()
        {
            do
            {
                ++this.nextUseAbilityType;
                this.nextUseAbilityType = (AbilityType)((int)this.nextUseAbilityType % 3);
                if (this.nextUseAbilityType == AbilityType.Vital &&
                    (
                        PlayerControl.GameOptions.MapId == 0 ||
                        PlayerControl.GameOptions.MapId == 1 ||
                        PlayerControl.GameOptions.MapId == 3
                    ))
                {
                    this.nextUseAbilityType = AbilityType.Admin;
                }
            }
            while (!this.canUseCrakingModule.Contains(this.nextUseAbilityType));
        }
        private void updateButtonSprite()
        {
            var button = this.Button as ChargableButton;

            Sprite sprite = Resources.Loader.CreateSpriteFromResources(
                Resources.Path.TestButton);

            switch (this.nextUseAbilityType)
            {
                case AbilityType.Admin:
                    sprite = this.adminSprite;
                    break;
                case AbilityType.Security:
                    sprite = this.securitySprite;
                    break;
                case AbilityType.Vital:
                    sprite = this.vitalSprite;
                    break;
                default:
                    break;
            }
            button.SetButtonImage(sprite);
        }

        private void showText(string text)
        {
            CachedPlayerControl.LocalPlayer.PlayerControl.StartCoroutine(
                coShowText(text).WrapToIl2Cpp());
        }

        private IEnumerator coShowText(string text)
        {
            if (this.tellText == null)
            {
                this.tellText = UnityEngine.Object.Instantiate(
                    FastDestroyableSingleton<HudManager>.Instance.TaskText,
                    Camera.main.transform, false);
                this.tellText.transform.localPosition = new Vector3(0.0f, -0.9f, -250.0f);
                this.tellText.alignment = TMPro.TextAlignmentOptions.Center;
                this.tellText.gameObject.layer = 5;
            }
            this.tellText.text = text;
            this.tellText.gameObject.SetActive(true);

            yield return new WaitForSeconds(3.5f);

            this.tellText.gameObject.SetActive(false);
        }

    }
}

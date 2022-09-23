using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using Hazel;

using Newtonsoft.Json.Linq;

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
            IsResetKillCoolWhenDollKill,
            DollKillCoolReduceRate,
            DefaultRedAbilityPart,
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
                HypnotistOption.HideArrowRange,
                10.0f, 5.0f, 25.0f, 0.5f,
                parentOps);

            CreateBoolOption(
                HypnotistOption.IsResetKillCoolWhenDollKill,
                true, parentOps);
            CreateIntOption(
                HypnotistOption.DollKillCoolReduceRate,
                20, 0, 75, 5,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                HypnotistOption.DefaultRedAbilityPart,
                0, 0, 10, 1,
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

            this.hideDistance = allOpt[
                GetRoleOptionId(HypnotistOption.HideArrowRange)].GetValue();
            this.isResetKillCoolWhenDollKill = allOpt[
                GetRoleOptionId(HypnotistOption.IsResetKillCoolWhenDollKill)].GetValue();
            this.dollKillCoolReduceRate = ((1.0f - (float)allOpt[
                GetRoleOptionId(HypnotistOption.DollKillCoolReduceRate)].GetValue()) / 100.0f);
            this.defaultRedAbilityPartNum = allOpt[
                GetRoleOptionId(HypnotistOption.DefaultRedAbilityPart)].GetValue();

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
                    bluePos.Add((vecPos, SystemConsoleType.SecurityCamera));
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
                    bluePos.Add((vecPos, SystemConsoleType.SecurityCamera));
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
                    grayPos.Add((vecPos, SystemConsoleType.SecurityCamera));
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
                    grayPos.Add((vecPos, SystemConsoleType.SecurityCamera));
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

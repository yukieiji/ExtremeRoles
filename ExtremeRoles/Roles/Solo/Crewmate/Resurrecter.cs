using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Resurrecter : SingleRoleBase, IRoleAwake<RoleTypes>, IRoleResetMeeting, IRoleOnRevive
    {
        public override bool IsAssignGhostRole
        {
            get => false;
        }

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum ResurrecterOption
        {
            AwakeTaskGage,
            ResurrectTaskGage,
            IsMeetingCoolResetOnResurrect,
            ResurrectDelayTime,
            CanResurrectOnExil,
            ResurrectTaskResetMeetingNum,
            ResurrectTaskResetGage,
        }

        public enum ResurrecterRpcOps : byte
        {
            UseResurrect,
            ReplaceTask,
            ResetFlash,
        }

        private bool awakeRole;
        private float awakeTaskGage;
        private bool awakeHasOtherVision;
        private float resurrectTaskGage;

        private bool canResurrect;
        private bool canResurrectOnExil;
        private bool isResurrected;
        private bool isExild;

        private bool isActiveMeetingCount;
        private int meetingCounter;
        private int maxMeetingCount;

        private bool activateResurrectTimer;
        private float resurrectTimer;

        private bool isMeetingCoolResetOnResurrect;

        private float resetTaskGage;
        private TMPro.TextMeshPro resurrectText;

        private static SpriteRenderer flash;

        public Resurrecter() : base(
            ExtremeRoleId.Resurrecter,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Resurrecter.ToString(),
            ColorPalette.SurvivorYellow,
            false, true, false, false)
        { }

        public static void RpcAbility(ref MessageReader reader)
        {
            ResurrecterRpcOps ops = (ResurrecterRpcOps)reader.ReadByte();
            byte resurrecterPlayerId = reader.ReadByte();

            switch (ops)
            {
                case ResurrecterRpcOps.UseResurrect:
                    Resurrecter resurrecter = ExtremeRoleManager.GetSafeCastedRole<Resurrecter>(
                        resurrecterPlayerId);
                    if (resurrecter == null) { return; }
                    useResurrect(resurrecter);
                    break;
                case ResurrecterRpcOps.ReplaceTask:
                    int index = reader.ReadInt32();
                    int taskIndex = reader.ReadInt32();
                    replaceToNewTask(resurrecterPlayerId, index, taskIndex);
                    break;
                case ResurrecterRpcOps.ResetFlash:
                    if (flash != null)
                    {
                        flash.enabled = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private static void useResurrect(Resurrecter resurrecter)
        {
            resurrecter.isResurrected = true;
            resurrecter.isActiveMeetingCount = true;
        }

        private static void replaceToNewTask(byte playerId, int index, int taskIndex)
        {
            var player = Player.GetPlayerControlById(playerId);

            if (player == null) { return; }

            byte taskId = (byte)taskIndex;

            if (GameSystem.SetPlayerNewTask(
                ref player, taskId, (uint)index))
            {
                player.Data.Tasks[index] = new GameData.TaskInfo(
                    taskId, (uint)index);
                player.Data.Tasks[index].Id = (uint)index;

                GameData.Instance.SetDirtyBit(
                    1U << (int)player.PlayerId);
            }
        }

        public void ResetOnMeetingStart()
        {
            if (this.isActiveMeetingCount)
            {
                ++this.meetingCounter;
            }

            if (this.resurrectText != null)
            {
                this.resurrectText.gameObject.SetActive(false);
            }

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.ResurrecterRpc,
                new List<byte>
                { 
                    (byte)ResurrecterRpcOps.ResetFlash,
                    CachedPlayerControl.LocalPlayer.PlayerId
                });

            if (flash != null)
            {
                flash.enabled = false;
            }
        }

        public void ResetOnMeetingEnd()
        {
            return;
        }

        public void ReviveAction(PlayerControl player)
        {
            // リセット会議クールダウン
            if (this.isMeetingCoolResetOnResurrect)
            {
                CachedShipStatus.Instance.EmergencyCooldown = 
                    (float)PlayerControl.GameOptions.EmergencyCooldown;
            }

            var role = ExtremeRoleManager.GetLocalPlayerRole();
            if (role.CanKill())
            {
                var hudManager = FastDestroyableSingleton<HudManager>.Instance;

                if (flash == null)
                {
                    flash = Object.Instantiate(
                         hudManager.FullScreen,
                         hudManager.transform);
                    flash.transform.localPosition = new Vector3(0f, 0f, 20f);
                    flash.gameObject.SetActive(true);
                }

                hudManager.StartCoroutine(
                    Effects.Lerp(1.0f, new System.Action<float>((p) =>
                    {
                        if (flash == null) { return; }
                        
                        if (p < 0.5)
                        {
                            flash.color = new Color(
                                this.NameColor.r, this.NameColor.g,
                                this.NameColor.b, Mathf.Clamp01(p * 2 * 0.75f));

                        }
                        else
                        {
                            flash.color = new Color(
                                this.NameColor.r, this.NameColor.g,
                                this.NameColor.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                        }
                        if (p == 1f)
                        {
                            flash.enabled = false;
                        }
                    }))
                );
            }
        }

        public string GetFakeOptionString() => "";

        public void Update(PlayerControl rolePlayer)
        {
            if (this.isActiveMeetingCount &&
                this.meetingCounter >= this.maxMeetingCount)
            {
                this.isActiveMeetingCount = false;
                this.meetingCounter = 0;
                replaceTask(rolePlayer);
                return;
            }
            
            if (rolePlayer.Data.IsDead && this.infoBlock())
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
            }

            if (!this.awakeRole || 
                (!this.canResurrect && !this.isResurrected))
            {
                float taskGage = Player.GetPlayerTaskGage(rolePlayer);

                if (taskGage >= this.awakeTaskGage && !this.awakeRole)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
                if (taskGage >= this.resurrectTaskGage && 
                    !this.canResurrect)
                {
                    if (rolePlayer.Data.IsDead)
                    {
                        revive(rolePlayer);
                    }
                    else
                    {
                        this.canResurrect = true;
                        this.isResurrected = false;
                    }
                }
            }

            if (this.isResurrected) { return; }

            if (rolePlayer.Data.IsDead &&
                this.activateResurrectTimer &&
                this.canResurrect &&
                MeetingHud.Instance != null &&
                ExileController.Instance != null)
            {
                if (this.resurrectText == null)
                {
                    this.resurrectText = Object.Instantiate(
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                        Camera.main.transform, false);
                    this.resurrectText.transform.localPosition = new Vector3(0.0f, 0.0f, -250.0f);
                    this.resurrectText.enableWordWrapping = false;
                }

                this.resurrectText.gameObject.SetActive(true);
                this.resurrectTimer -= Time.fixedDeltaTime;
                this.resurrectText.text = Translation.GetString(
                    string.Format("resurrectText", this.resurrectTimer));

                if (this.resurrectTimer <= 0.0f)
                {
                    this.activateResurrectTimer = false;
                    revive(rolePlayer);
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
            this.isExild = true;

            if (this.canResurrectOnExil &&
                this.canResurrect && 
                !this.isResurrected)
            {
                this.activateResurrectTimer = true;
            }
        }

        public override void RolePlayerKilledAction(
            PlayerControl rolePlayer,
            PlayerControl killerPlayer)
        {
            this.isExild = false;
            if (this.canResurrect && 
                !this.isResurrected)
            {
                this.activateResurrectTimer = true;
            }
        }

        public override bool IsBlockShowMeetingRoleInfo() => this.infoBlock();

        public override bool IsBlockShowPlayingRoleInfo() => this.infoBlock();
             

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                ResurrecterOption.AwakeTaskGage,
                100, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                ResurrecterOption.ResurrectTaskGage,
                100, 70, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);

            CreateBoolOption(
                ResurrecterOption.IsMeetingCoolResetOnResurrect,
                true, parentOps);

            CreateFloatOption(
                ResurrecterOption.ResurrectDelayTime,
                3.0f, 0.0f, 10.0f, 0.1f,
                parentOps);

            CreateIntOption(
                ResurrecterOption.ResurrectTaskResetMeetingNum,
                1, 1, 5, 1,
                parentOps);

            CreateIntOption(
                ResurrecterOption.ResurrectTaskResetGage,
                20, 10, 50, 5,
                parentOps,
                format: OptionUnit.Percentage);

            CreateBoolOption(
                ResurrecterOption.CanResurrectOnExil,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;

            this.awakeTaskGage = (float)allOpt[
                GetRoleOptionId(ResurrecterOption.AwakeTaskGage)].GetValue() / 100.0f;
            this.resurrectTaskGage = (float)allOpt[
                GetRoleOptionId(ResurrecterOption.ResurrectTaskGage)].GetValue() / 100.0f;
            this.resetTaskGage = (float)allOpt[
                GetRoleOptionId(ResurrecterOption.ResurrectTaskResetGage)].GetValue() / 100.0f;

            this.resurrectTimer = allOpt[
                GetRoleOptionId(ResurrecterOption.ResurrectDelayTime)].GetValue();
            this.canResurrectOnExil = allOpt[
                GetRoleOptionId(ResurrecterOption.CanResurrectOnExil)].GetValue();
            this.maxMeetingCount = allOpt[
                GetRoleOptionId(ResurrecterOption.ResurrectTaskResetMeetingNum)].GetValue();
            this.isMeetingCoolResetOnResurrect = allOpt[
                GetRoleOptionId(ResurrecterOption.IsMeetingCoolResetOnResurrect)].GetValue();

            this.awakeHasOtherVision = this.HasOtherVison;
            this.canResurrect = false;
            this.isResurrected = false;

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

        private bool infoBlock()
        {
            if (this.isExild)
            {
                return this.canResurrectOnExil && !this.isResurrected;
            }
            else
            {
                return !this.isResurrected;
            }
        }

        private void revive(PlayerControl rolePlayer)
        {
            if (rolePlayer == null) { return; }

            byte playerId = rolePlayer.PlayerId;

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.UncheckedRevive,
                new List<byte> { playerId });
            RPCOperator.UncheckedRevive(playerId);

            if (rolePlayer.Data == null ||
                rolePlayer.Data.IsDead ||
                rolePlayer.Data.Disconnected) { return; }

            var allPlayer = GameData.Instance.AllPlayers;
            ShipStatus ship = CachedShipStatus.Instance;

            List<Vector2> randomPos = new List<Vector2>();

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                randomPos = ExtremeRolesPlugin.Compat.ModMap.GetSpawnPos(
                    playerId);
            }
            else
            {
                switch (PlayerControl.GameOptions.MapId)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        Vector2 baseVec = Vector2.up;
                        baseVec = baseVec.Rotate(
                            (float)(playerId - 1) * (360f / (float)allPlayer.Count));
                        Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);
                        randomPos.Add(ship.InitialSpawnCenter + offset);
                        randomPos.Add(ship.MeetingSpawnCenter + offset);
                        break;
                    case 4:
                        randomPos.Add(new Vector2(-0.7f, 8.5f));
                        randomPos.Add(new Vector2(-0.7f, -1.0f));
                        randomPos.Add(new Vector2(15.5f, 0.0f));
                        randomPos.Add(new Vector2(-7.0f, -11.5f));
                        randomPos.Add(new Vector2(20.0f, 10.5f));
                        randomPos.Add(new Vector2(33.5f, -1.5f));
                        break;
                    default:
                        break;
                }
            }

            Vector2 teleportPos = randomPos[
                RandomGenerator.Instance.Next(randomPos.Count)];

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                (byte)RPCOperator.Command.UncheckedSnapTo,
                Hazel.SendOption.Reliable, -1);
            writer.Write(playerId);
            writer.Write(teleportPos.x);
            writer.Write(teleportPos.y);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.UncheckedSnapTo(playerId, teleportPos);

            RPCOperator.Call(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                RPCOperator.Command.ResurrecterRpc,
                new List<byte> { (byte)ResurrecterRpcOps.UseResurrect, playerId });
            useResurrect(this);

            FastDestroyableSingleton<HudManager>.Instance.Chat.chatBubPool.ReclaimAll();
            if (this.resurrectText != null)
            {
                this.resurrectText.gameObject.SetActive(false);
            }
        }

        private void replaceTask(PlayerControl rolePlayer)
        {
            GameData.PlayerInfo playerInfo = rolePlayer.Data;

            var shuffleTaskIndex = Enumerable.Range(
                0, playerInfo.Tasks.Count).ToList().OrderBy(
                    item => RandomGenerator.Instance.Next()).ToList();

            int replaceTaskNum = 1;
            int maxReplaceTaskNum = Mathf.CeilToInt(playerInfo.Tasks.Count * this.resetTaskGage);

            foreach (int i in shuffleTaskIndex)
            {
                if (replaceTaskNum >= maxReplaceTaskNum) { break; }

                if (playerInfo.Tasks[i].Complete)
                {
                    
                    int taskIndex;
                    int replaceTaskId = playerInfo.Tasks[i].TypeId;

                    if (CachedShipStatus.Instance.CommonTasks.FirstOrDefault(
                    (NormalPlayerTask t) => t.Index == replaceTaskId) != null)
                    {
                        taskIndex = GameSystem.GetRandomCommonTaskId();
                    }
                    else if (CachedShipStatus.Instance.LongTasks.FirstOrDefault(
                        (NormalPlayerTask t) => t.Index == replaceTaskId) != null)
                    {
                        taskIndex = GameSystem.GetRandomLongTask();
                    }
                    else if (CachedShipStatus.Instance.NormalTasks.FirstOrDefault(
                        (NormalPlayerTask t) => t.Index == replaceTaskId) != null)
                    {
                        taskIndex = GameSystem.GetRandomNormalTaskId();
                    }
                    else
                    {
                        continue;
                    }

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        (byte)RPCOperator.Command.AgencySetNewTask,
                        Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)ResurrecterRpcOps.ReplaceTask);
                    writer.Write(rolePlayer.PlayerId);
                    writer.Write(i);
                    writer.Write(taskIndex);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    replaceToNewTask(rolePlayer.PlayerId, i, taskIndex);
                    
                    ++replaceTaskNum;
                }
            }
        }
    }
}

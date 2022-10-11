using System.Collections.Generic;

using UnityEngine;
using Hazel;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Delusioner : 
        SingleRoleBase, 
        IRoleAbility,
        IRoleAwake<RoleTypes>,
        IRoleVoteModifier
    {
        public int Order => (int)IRoleVoteModifier.ModOrder.DelusionerCheckVote;

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.isAwakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public RoleAbilityButtonBase Button
        { 
            get => this.deflectDamageButton; 
            set
            {
                this.deflectDamageButton = value;
            }
        }

        public enum DelusionerOption
        {
            AwakeVoteNum,
            IsOnetimeAwake,
            Range,
            VoteCoolTimeReduceRate,
            DeflectDamagePenaltyRate,
            IsIncludeLocalPlayer,
            IsIncludeSpawnPoint
        }

        private RoleAbilityButtonBase deflectDamageButton;

        private bool isAwakeRole;
        private bool isOneTimeAwake;

        private float range;

        private byte targetPlayerId;

        private int awakeVoteCount;
        private int curVoteCount;

        private bool includeLocalPlayer;
        private bool includeSpawnPoint;

        private float defaultCoolTime;
        private float curCoolTime;
        
        private int voteCoolTimeReduceRate;
        private float deflectDamagePenaltyMod;

        private List<Vector2> airShipSpawn;

        public Delusioner() : base(
            ExtremeRoleId.Delusioner,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Delusioner.ToString(),
            ColorPalette.DelusionerPink,
            false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateAbilityCountButton(
                Translation.GetString("deflectDamage"),
                Loader.CreateSpriteFromResources(
                    Path.DelusionerDeflectDamage));
            this.Button.SetLabelToCrewmate();

            this.airShipSpawn = GameSystem.GetAirShipRandomSpawn();
        }

        public string GetFakeOptionString() => "";

        public bool IsAbilityUse()
        {
            if (!this.IsAwake) { return false; }

            this.targetPlayerId = byte.MaxValue;

            PlayerControl target = Player.GetClosestPlayerInRange(
                CachedPlayerControl.LocalPlayer, this,
                this.range);
            if (target == null) { return false; }

            this.targetPlayerId = target.PlayerId;

            return this.IsCommonUse();
        }

        public void ModifiedVote(
            byte rolePlayerId,
            ref Dictionary<byte, byte> voteTarget,
            ref Dictionary<byte, int> voteResult)
        {
            return;
        }

        public void ModifiedVoteAnime(
            MeetingHud instance,
            GameData.PlayerInfo rolePlayer,
            ref Dictionary<byte, int> voteIndex)
        {
            if (voteIndex.TryGetValue(
                rolePlayer.PlayerId,
                out int forRolePlayerVote))
            {
                this.curVoteCount = this.curVoteCount + forRolePlayerVote;
                this.isAwakeRole = this.curVoteCount >= this.awakeVoteCount;
                if (this.Button != null &&
                    this.voteCoolTimeReduceRate > 0)
                {
                    int curVoteCooltimeReduceRate = this.voteCoolTimeReduceRate * forRolePlayerVote;

                    this.Button.SetAbilityCoolTime(
                        this.defaultCoolTime * ((100.0f - (float)curVoteCooltimeReduceRate) / 100.0f));
                }
            }

            if (this.isAwakeRole &&
                this.isOneTimeAwake)
            {
                this.curVoteCount = 0;
            }
        }

        public void ResetModifier()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            this.curCoolTime = this.defaultCoolTime;
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.isAwakeRole)
            {
                this.Button?.SetActive(false);
            }
        }

        public bool UseAbility()
        {
            List<Vector2> randomPos = new List<Vector2>();
            byte teloportTarget = this.targetPlayerId;

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
            var allPlayer = GameData.Instance.AllPlayers;
            ShipStatus ship = CachedShipStatus.Instance;

            if (this.includeLocalPlayer)
            {
                randomPos.Add(localPlayer.transform.position);
            }

            if (this.includeSpawnPoint)
            {
                
                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    randomPos = ExtremeRolesPlugin.Compat.ModMap.GetSpawnPos(
                        teloportTarget);
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
                                (float)(teloportTarget - 1) * (360f / (float)allPlayer.Count));
                            Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);
                            randomPos.Add(ship.InitialSpawnCenter + offset);
                            randomPos.Add(ship.MeetingSpawnCenter + offset);
                            break;
                        case 4:
                            randomPos.AddRange(this.airShipSpawn);
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (GameData.PlayerInfo player in allPlayer.GetFastEnumerator())
            {
                if (player == null) { continue; }
                if (!player.Disconnected &&
                    player.PlayerId != localPlayer.PlayerId && 
                    player.PlayerId != teloportTarget &&
                    !player.IsDead &&
                    player.Object != null &&
                    !player.Object.inVent)
                {
                    randomPos.Add(player.Object.transform.position);
                }
            }

            Vector2 teleportPos = randomPos[
                RandomGenerator.Instance.Next(randomPos.Count)];

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                localPlayer.NetId,
                (byte)RPCOperator.Command.UncheckedSnapTo,
                Hazel.SendOption.Reliable, -1);
            writer.Write(teloportTarget);
            writer.Write(teleportPos.x);
            writer.Write(teleportPos.y);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCOperator.UncheckedSnapTo(this.targetPlayerId, teleportPos);

            if (this.Button != null &&
                this.deflectDamagePenaltyMod < 1.0f)
            {
                this.curCoolTime = this.curCoolTime * this.deflectDamagePenaltyMod;
                this.Button.SetAbilityCoolTime(this.curCoolTime);
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

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            CreateIntOption(
                DelusionerOption.AwakeVoteNum,
                3, 0, 8, 1, parentOps,
                format: OptionUnit.VoteNum);
            CreateBoolOption(
                DelusionerOption.IsOnetimeAwake,
                false, parentOps);

            CreateFloatOption(
                DelusionerOption.Range,
                2.5f, 0.0f, 7.5f, 0.1f,
                parentOps);

            this.CreateAbilityCountOption(
                parentOps, 3, 25);

            CreateIntOption(
                DelusionerOption.VoteCoolTimeReduceRate,
                5, 0, 100, 5, parentOps,
                format: OptionUnit.Percentage);
            CreateIntOption(
                DelusionerOption.DeflectDamagePenaltyRate,
                10, 0, 100, 5, parentOps,
                format: OptionUnit.Percentage);

            CreateBoolOption(
                DelusionerOption.IsIncludeLocalPlayer,
                true, parentOps);
            CreateBoolOption(
                DelusionerOption.IsIncludeSpawnPoint,
                false, parentOps);

        }

        protected override void RoleSpecificInit()
        {
            var allOpt = OptionHolder.AllOption;
            this.awakeVoteCount = allOpt[
                GetRoleOptionId(DelusionerOption.AwakeVoteNum)].GetValue();
            this.isOneTimeAwake = allOpt[
                GetRoleOptionId(DelusionerOption.IsOnetimeAwake)].GetValue();
            this.voteCoolTimeReduceRate = allOpt[
                GetRoleOptionId(DelusionerOption.VoteCoolTimeReduceRate)].GetValue();
            this.deflectDamagePenaltyMod = ((100f - (float)allOpt[
                GetRoleOptionId(DelusionerOption.DeflectDamagePenaltyRate)].GetValue()) / 100f);
            this.range = allOpt[
                GetRoleOptionId(DelusionerOption.Range)].GetValue();

            this.includeLocalPlayer = allOpt[
                GetRoleOptionId(DelusionerOption.IsIncludeLocalPlayer)].GetValue();
            this.includeSpawnPoint = allOpt[
                GetRoleOptionId(DelusionerOption.IsIncludeSpawnPoint)].GetValue();

            this.isOneTimeAwake = this.isOneTimeAwake && this.awakeVoteCount > 0;
            this.defaultCoolTime = allOpt[
                GetRoleOptionId(RoleAbilityCommonOption.AbilityCoolTime)].GetValue();
            this.curCoolTime = this.defaultCoolTime;
            this.isAwakeRole = this.awakeVoteCount == 0;
            if (this.isAwakeRole)
            {
                this.isOneTimeAwake = false;
            }
            this.RoleAbilityInit();
        }
    }
#if DEBUG
    [HarmonyLib.HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static class AirShipSpawnCheck
    {
        public static void Postfix(SpawnInMinigame __instance)
        {
            if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
            {
                foreach (SpawnInMinigame.SpawnLocation pos in __instance.Locations)
                {
                    Logging.Debug($"Name:{pos.Name}  Pos:{pos.Location}");
                }
            }
        }
    }
#endif
}

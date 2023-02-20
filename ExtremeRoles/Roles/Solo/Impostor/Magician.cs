using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public sealed class Magician : SingleRoleBase, IRoleAbility
    {

        public ExtremeAbilityButton Button
        { 
            get => this.jugglingButton;
            set
            {
                this.jugglingButton = value;
            }
        }

        public enum MagicianOption
        {
            TeleportTargetRate,
            DupeTeleportTargetTo,
            IncludeRolePlayer,
            IncludeSpawnPoint
        }

        private float teleportRate = 1.0f;
        private bool dupeTeleportTarget = true;
        private bool includeRolePlayer = true;
        private bool includeSpawnPoint = true;

        private ExtremeAbilityButton jugglingButton;

        private List<Vector2> airShipSpawn;

        public Magician() : base(
            ExtremeRoleId.Magician,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Magician.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                "juggling",
                Loader.CreateSpriteFromResources(
                    Path.MagicianJuggling));

            this.airShipSpawn = GameSystem.GetAirShipRandomSpawn();
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            return;
        }

        public bool UseAbility()
        {
            // まずはテレポート先とかにも設定できるプレヤーを取得
            var allPlayer = CachedPlayerControl.AllPlayerControls;
            var validPlayer = allPlayer.Where(x => 
                x != null && 
                x.Data != null && 
                !x.Data.IsDead &&
                !x.Data.Disconnected &&
                !x.PlayerControl.inVent && // ベント入ってない
                x.PlayerControl.moveable &&  // 移動できる状態か
                (CachedPlayerControl.LocalPlayer.PlayerId != x.PlayerId ||
                this.includeRolePlayer));

            var teleportPlayer = validPlayer.OrderBy(
                x => RandomGenerator.Instance.Next()).Take(
                    (int)Math.Ceiling(validPlayer.Count() * this.teleportRate));

            // テレポートする人が存在しない場合
            if (!teleportPlayer.Any()) { return false; }

            var targetPos = validPlayer.Select(x =>
            (
                new Vector2(x.transform.position.x, x.transform.position.y)
            ));

            byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
                ByteOptionNames.MapId);

            List<Vector2> additionalPos = new List<Vector2>();

            if (mapId == 4)
            {
                targetPos = targetPos.Where(
                    item =>
                        (-26f > item.x || item.x > -24f) ||
                        (39f > item.y || item.y > 41f));
            }

            byte randomPlayer = teleportPlayer.First().PlayerId;

            if (this.includeSpawnPoint)
            {
                var ship = CachedShipStatus.Instance;

                if (ExtremeRolesPlugin.Compat.IsModMap)
                {
                    additionalPos = ExtremeRolesPlugin.Compat.ModMap.GetSpawnPos(
                        randomPlayer);
                }
                else
                {
                    switch (mapId)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            Vector2 baseVec = Vector2.up;
                            baseVec = baseVec.Rotate(
                                (float)(randomPlayer - 1) * (360f / (float)allPlayer.Count));
                            Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);
                            additionalPos.Add(ship.InitialSpawnCenter + offset);
                            additionalPos.Add(ship.MeetingSpawnCenter + offset);
                            break;
                        case 4:
                            additionalPos.AddRange(this.airShipSpawn);
                            break;
                        default:
                            break;
                    }
                }
                targetPos = targetPos.Concat(additionalPos);
            }

            if (!targetPos.Any()) { return false; }

            if (this.dupeTeleportTarget)
            {
                int size = targetPos.Count();
                foreach (var player in teleportPlayer)
                {
                    Player.RpcUncheckSnap(player.PlayerId, targetPos.ElementAt(
                        RandomGenerator.Instance.Next(size)));
                }
            }
            else
            {
                teleportPlayer = teleportPlayer.OrderBy(x => RandomGenerator.Instance.Next());
                foreach (var item in targetPos.Select((pos, index) => new { pos, index }))
                {
                    var player = teleportPlayer.ElementAtOrDefault(item.index);
                    if (player == null) { break; }
                    Player.RpcUncheckSnap(player.PlayerId, item.pos);
                }
            }

            return true;
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            this.CreateCommonAbilityOption(parentOps);

            CreateIntOption(
                MagicianOption.TeleportTargetRate,
                100, 10, 100, 10, parentOps,
                format: OptionUnit.Percentage);
            CreateBoolOption(
                MagicianOption.DupeTeleportTargetTo,
                true, parentOps);
            CreateBoolOption(
                MagicianOption.IncludeSpawnPoint,
                false, parentOps);
            CreateBoolOption(
                MagicianOption.IncludeRolePlayer,
                false, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            var allOption = OptionHolder.AllOption;
            this.teleportRate = allOption[
                GetRoleOptionId(MagicianOption.TeleportTargetRate)].GetValue();
            this.dupeTeleportTarget = allOption[
                GetRoleOptionId(MagicianOption.DupeTeleportTargetTo)].GetValue();
            this.includeRolePlayer = allOption[
                GetRoleOptionId(MagicianOption.IncludeSpawnPoint)].GetValue();
            this.includeSpawnPoint = allOption[
                GetRoleOptionId(MagicianOption.IncludeRolePlayer)].GetValue();

            this.RoleAbilityInit();
        }
    }
}

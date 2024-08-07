﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using UnityEngine;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour.Minigames;
using ExtremeRoles.Module.Ability;




using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.Roles.Solo.Impostor;

public sealed class Magician : SingleRoleBase, IRoleAutoBuildAbility
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

    public Magician() : base(
        ExtremeRoleId.Magician,
        ExtremeRoleType.Impostor,
        ExtremeRoleId.Magician.ToString(),
        Palette.ImpostorRed,
        true, false, true, true)
    { }

    public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "juggling",
			Resources.UnityObjectLoader.LoadSpriteFromResources(
				ObjectPath.MagicianJuggling));
    }

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

    public void ResetOnMeetingEnd(NetworkedPlayerInfo exiledPlayer = null)
    {
        return;
    }

    public void ResetOnMeetingStart()
    {
        return;
    }

    public bool UseAbility()
    {
        // まずはテレポート先とかにも設定できるプレヤーを取得
        var validPlayer = PlayerCache.AllPlayerControl.Where(x =>
            x != null &&
            x.Data != null &&
            !x.Data.IsDead &&
            !x.Data.Disconnected &&
			!x.inVent && // ベント入ってない
			x.moveable &&  // 移動できる状態か
			!x.inMovingPlat && // なんか乗ってないか
			(PlayerControl.LocalPlayer.PlayerId != x.PlayerId || this.includeRolePlayer));

        var teleportPlayer = validPlayer.OrderBy(
            x => RandomGenerator.Instance.Next()).Take(
                (int)Math.Ceiling(validPlayer.Count() * this.teleportRate));

        // テレポートする人が存在しない場合
        if (!teleportPlayer.Any()) { return false; }

        var targetPos = validPlayer.Select(x =>
        (
            new Vector2(x.transform.position.x, x.transform.position.y)
        ));

		targetPos = targetPos.Where(item => !ExtremeSpawnSelectorMinigame.IsCloseWaitPos(item));

		byte randomPlayer = teleportPlayer.First().PlayerId;

        if (this.includeSpawnPoint)
        {
			Map.AddSpawnPoint(targetPos, randomPlayer);
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

    protected override void CreateSpecificOption(AutoParentSetOptionCategoryFactory factory)
    {
        IRoleAbility.CreateAbilityCountOption(factory, 1, 10);

        factory.CreateIntOption(
            MagicianOption.TeleportTargetRate,
            100, 10, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateBoolOption(
            MagicianOption.DupeTeleportTargetTo,
            true);
        factory.CreateBoolOption(
            MagicianOption.IncludeSpawnPoint,
            false);
        factory.CreateBoolOption(
            MagicianOption.IncludeRolePlayer,
            false);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;
        this.teleportRate = (float)cate.GetValue<MagicianOption, int>(
            MagicianOption.TeleportTargetRate) / 100.0f;
        this.dupeTeleportTarget = cate.GetValue<MagicianOption, bool>(
            MagicianOption.DupeTeleportTargetTo);
        this.includeRolePlayer = cate.GetValue<MagicianOption, bool>(
            MagicianOption.IncludeSpawnPoint);
        this.includeSpawnPoint = cate.GetValue<MagicianOption, bool>(
            MagicianOption.IncludeRolePlayer);
    }
}

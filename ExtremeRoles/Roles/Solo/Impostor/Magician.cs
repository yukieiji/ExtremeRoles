using System;
using System.Linq;
using System.Data;

using UnityEngine;
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

    public ExtremeAbilityButton Button { get; set; }

    public enum MagicianOption
    {
        TeleportTargetRate,
        DupeTeleportTargetTo,
        IncludeRolePlayer,
        IncludeSpawnPoint
    }

	private AbilityParameter parameter;

	public readonly record struct AbilityParameter(
		float TeleportTargetRate,
		bool DupeTeleportTarget,
		bool IncludeRolePlayer,
		bool IncludeSpawnPoint);

    public Magician() : base(
		RoleCore.BuildImpostor(ExtremeRoleId.Magician),
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
		=> UseAbility(this.parameter);

	public static bool UseAbility(AbilityParameter param)
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
			(PlayerControl.LocalPlayer.PlayerId != x.PlayerId || param.IncludeRolePlayer));

		var teleportPlayer = validPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).Take(
				(int)Math.Ceiling(validPlayer.Count() * param.TeleportTargetRate));

		// テレポートする人が存在しない場合
		if (!teleportPlayer.Any()) { return false; }

		var targetPos = validPlayer.Select(x =>
		(
			new Vector2(x.transform.position.x, x.transform.position.y)
		));

		targetPos = targetPos.Where(item => !ExtremeSpawnSelectorMinigame.IsCloseWaitPos(item));

		byte randomPlayer = teleportPlayer.First().PlayerId;

		if (param.IncludeSpawnPoint)
		{
			Map.AddSpawnPoint(targetPos, randomPlayer);
		}

		if (!targetPos.Any()) { return false; }

		if (param.DupeTeleportTarget)
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

        factory.CreateNewIntOption(
            MagicianOption.TeleportTargetRate,
            100, 10, 100, 10,
            format: OptionUnit.Percentage);
        factory.CreateNewBoolOption(
            MagicianOption.DupeTeleportTargetTo,
            true);
        factory.CreateNewBoolOption(
            MagicianOption.IncludeSpawnPoint,
            false);
        factory.CreateNewBoolOption(
            MagicianOption.IncludeRolePlayer,
            false);
    }

    protected override void RoleSpecificInit()
    {
        var cate = this.Loader;

		this.parameter = new AbilityParameter(
			(float)cate.GetValue<MagicianOption, int>(MagicianOption.TeleportTargetRate) / 100.0f,
			cate.GetValue<MagicianOption, bool>(MagicianOption.DupeTeleportTargetTo),
			cate.GetValue<MagicianOption, bool>(MagicianOption.IncludeRolePlayer),
			cate.GetValue<MagicianOption, bool>(MagicianOption.IncludeSpawnPoint)
		);
    }
}

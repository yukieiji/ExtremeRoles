﻿using Hazel;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.Neutral;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral;

public sealed class Artist :
	SingleRoleBase,
	IRoleAbility,
	IRoleUpdate,
	IRoleSpecialReset
{
    public enum AliceOption
    {
        CanUseSabotage,
        RevartCommonTaskNum,
        RevartLongTaskNum,
        RevartNormalTaskNum,
    }

	public enum Ops : byte
	{
		Start,
		End
	}

    public ExtremeAbilityButton? Button { get; set; }
	private float area = 0.0f;
	private ArtistLineDrawer? drawer;

	private float winArea = 0.0f;

    public Artist(): base(
        ExtremeRoleId.Artist,
        ExtremeRoleType.Neutral,
        ExtremeRoleId.Artist.ToString(),
        ColorPalette.AliceGold,
        true, false, true, true)
    { }

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null ||
			rolePlayer.Data == null ||
			this.IsWin)
		{
			return;
		}

		if (this.drawer != null)
		{
			this.IsWin = this.area + this.drawer.Area >= this.winArea;
			if (this.IsWin)
			{
				ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
			}
		}
	}

	public void CreateAbility()
    {
        this.CreateAbilityCountButton(
            "shipBroken", Loader.CreateSpriteFromResources(
                Path.AliceShipBroken));
    }

    public bool IsAbilityUse() => this.IsCommonUse();

	public bool UseAbility()
    {
		drawOps(CachedPlayerControl.LocalPlayer);
		return true;
    }

	public void AllReset(PlayerControl rolePlayer)
	{
		endLine(this);
		this.IsWin = false;
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
		this.IsNeutralSameTeam(targetRole);

	public static void DrawOps(in MessageReader reader)
    {
		byte playerId = reader.ReadByte();
		Ops ops = (Ops)reader.ReadByte();

		Artist artist = ExtremeRoleManager.GetSafeCastedRole<Artist>(playerId);
		PlayerControl artistPlayer = Player.GetPlayerControlById(playerId);
		if (artist == null || artistPlayer == null) { return; }

		switch (ops)
		{
			case Ops.Start:
				startLine(artist, artistPlayer);
				break;
			case Ops.End:
				endLine(artist);
				break;
			default:
				break;
		}
    }

    protected override void CreateSpecificOption(
        IOptionInfo parentOps)
    {

        this.CreateAbilityCountOption(
            parentOps, 2, 100);

    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

		this.area = 0.0f;
        this.RoleAbilityInit();
    }

    public void ResetOnMeetingStart()
    {
		if (this.drawer == null) { return; }
		drawOps(CachedPlayerControl.LocalPlayer);
    }

    public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
    {
        return;
    }

	private void drawOps(PlayerControl playerControl)
	{
		bool isStart = this.drawer == null;
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.AcceleratorAbility))
		{
			caller.WriteByte((byte)(isStart ? Ops.Start : Ops.End));
			caller.WriteByte(playerControl.PlayerId);
		}

		if (this.drawer == null)
		{
			startLine(this, playerControl);
		}
		else
		{
			endLine(this);
		}
	}

	private static void startLine(Artist artist, PlayerControl artistPlayer)
	{
		if (artist.drawer != null) { return; }
		GameObject obj = new GameObject("Artist_Line");
		artist.drawer = obj.AddComponent<ArtistLineDrawer>();
		artist.drawer.ArtistPlayer = artistPlayer;
	}

	private static void endLine(Artist artist)
	{
		if (artist.drawer == null) { return; }
		artist.area += artist.drawer.Area;
		artist.IsWin = artist.area >= artist.winArea;
		Object.Destroy(artist.drawer);
		artist.drawer = null;
	}
}

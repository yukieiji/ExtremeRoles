using Hazel;
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
	IRoleAutoBuildAbility,
	IRoleUpdate,
	IRoleSpecialReset
{
    public enum ArtistOption
	{
		CanUseVent,
		WinAreaSize
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
        ColorPalette.ArtistChenChuWhowan,
        false, false, false, false)
    { }

	public void Update(PlayerControl rolePlayer)
	{
		if (rolePlayer == null ||
			rolePlayer.Data == null ||
			rolePlayer.Data.IsDead ||
			rolePlayer.Data.Disconnected ||
			this.IsWin ||
			this.drawer == null)
		{
			return;
		}

		this.IsWin = this.area + this.drawer.Area >= this.winArea;
		if (this.IsWin)
		{
			ExtremeRolesPlugin.ShipState.RpcRoleIsWin(rolePlayer.PlayerId);
		}
	}

	public void CreateAbility()
    {
		this.CreatePassiveAbilityButton(
			"ArtistArtOn", "ArtistArtOff",
			Loader.CreateSpriteFromResources(
			   Path.ArtistArtOn),
			Loader.CreateSpriteFromResources(
			   Path.ArtistArtOff),
			this.CleanUp,
			() =>
			{
				PlayerControl? pc = CachedPlayerControl.LocalPlayer;

				return
					!(
						pc == null ||
						pc.Data == null ||
						pc.Data.IsDead ||
						pc.Data.Disconnected ||
						pc.inMovingPlat ||
						pc.inVent ||
						pc.onLadder
					);
			});
	}

    public bool IsAbilityUse() => IRoleAbility.IsCommonUse();

	public bool UseAbility()
    {
		drawOps(CachedPlayerControl.LocalPlayer);
		return true;
    }

	public void CleanUp()
	{
		drawOps(CachedPlayerControl.LocalPlayer);
	}

	public void AllReset(PlayerControl rolePlayer)
	{
		endLine(this);
		this.IsWin = false;
	}

	public override bool IsSameTeam(SingleRoleBase targetRole) =>
		this.IsNeutralSameTeam(targetRole);

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		endLine(this);
	}

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
		CreateBoolOption(
			ArtistOption.CanUseVent,
			false, parentOps);
		CreateIntOption(
			ArtistOption.WinAreaSize,
			15, 1, 100, 1, parentOps);
        this.CreateCommonAbilityOption(
            parentOps, 3.0f);

    }

    protected override void RoleSpecificInit()
    {
        var allOption = OptionManager.Instance;

		this.area = 0.0f;
		this.UseVent = allOption.GetValue<bool>(
			GetRoleOptionId(ArtistOption.CanUseVent));
		this.winArea = allOption.GetValue<int>(
			GetRoleOptionId(ArtistOption.WinAreaSize));
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
			RPCOperator.Command.ArtistRpcOps))
		{
			caller.WriteByte(playerControl.PlayerId);
			caller.WriteByte((byte)(isStart ? Ops.Start : Ops.End));
		}

		if (isStart)
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

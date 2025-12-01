using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Helper;

using TempWinData = Il2CppSystem.Collections.Generic.List<CachedPlayerData>;
using Player = NetworkedPlayerInfo;

namespace ExtremeRoles.Module.GameResult;

public sealed class WinnerContainer
{
	public readonly record struct Result(
		IReadOnlyList<CachedPlayerData> Winner,
		IReadOnlyList<Player> PlusedWinner);

	public TempWinData DefaultWinPlayer => EndGameResult.CachedWinners;

	public IReadOnlyList<Player> PlusedWinner => plusWinPlayr;

	private readonly Dictionary<byte, CachedPlayerData> allWinnerPool = new Dictionary<byte, CachedPlayerData>();
	private readonly List<CachedPlayerData> finalWinPlayer = new List<CachedPlayerData>();
	private readonly List<Player> plusWinPlayr = new List<Player>();

	public void SetWinner()
	{
		finalWinPlayer.Clear();
		plusWinPlayr.Clear();

		finalWinPlayer.AddRange(DefaultWinPlayer.ToArray());
		plusWinPlayr.AddRange(ExtremeRolesPlugin.ShipState.GetPlusWinner());
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		builder
			.AppendLine("---- Current Win data ----")
			.AppendLine("--- Default Winner ---");

		foreach (var winner in DefaultWinPlayer)
		{
			builder.AppendLine($"PlayerName:{winner.PlayerName}");
		}

		builder.AppendLine("--- Final Winner ---");
		foreach (var winner in finalWinPlayer)
		{
			builder.AppendLine($"PlayerName:{winner.PlayerName}");
		}

		builder.AppendLine("--- Plus Winner ---");
		foreach (var winner in plusWinPlayr)
		{
			builder.AppendLine($"PlayerName:{winner.PlayerName}");
		}

		return builder.ToString();
	}

	public Result Convert() => new Result(finalWinPlayer, plusWinPlayr);

	public void AllClear()
	{
		finalWinPlayer.Clear();
		plusWinPlayr.Clear();
	}

	public void Clear()
	{
		finalWinPlayer.Clear();
	}

	public void RemoveAll(Player playerInfo)
	{
		string playerName = playerInfo.PlayerName;
		Logging.Debug($"Remove [{playerName}] from all winner pool");
		plusWinPlayr.RemoveAll(x => x.PlayerName == playerName);
		Remove(playerInfo);
	}
	public void Remove(CachedPlayerData player)
	{
		finalWinPlayer.RemoveAll(x => x.PlayerName == player.PlayerName);
	}
	public void Remove(Player player)
	{
		finalWinPlayer.RemoveAll(x => x.PlayerName == player.PlayerName);
	}

	public void AddWithPlus(Player playerInfo)
	{
		Logging.Debug($"Add [{playerInfo.PlayerName}] winner pool(With and Plus)");
		Add(playerInfo);
		AddPlusWinner(playerInfo);
	}

	public void Add(Player playerInfo)
	{
		if (!allWinnerPool.TryGetValue(playerInfo.PlayerId, out var wpd) ||
			wpd == null)
		{
			ExtremeRolesPlugin.Logger.LogError($"Can't find {playerInfo.PlayerName} in winner pool");
			return;
		}
		finalWinPlayer.Add(wpd);
	}
	public void AddPlusWinner(Player player)
	{
		plusWinPlayr.Add(player);
	}

	public void AddPool(Player playerInfo)
	{
		CachedPlayerData wpd = new CachedPlayerData(playerInfo);
		allWinnerPool.Add(playerInfo.PlayerId, wpd);
	}

	public bool Contains(string name)
	{
		foreach (var win in finalWinPlayer)
		{
			if (win.PlayerName == name)
			{
				return true;
			}
		}

		foreach (var win in plusWinPlayr)
		{
			if (win.PlayerName == name)
			{
				return true;
			}
		}
		return false;
	}
}

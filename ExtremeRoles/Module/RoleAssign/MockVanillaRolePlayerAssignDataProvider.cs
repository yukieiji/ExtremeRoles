using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;


namespace ExtremeRoles.Module.RoleAssign;

public sealed class MockVanillaRolePlayerAssignDataProvider : IVanillaRolePlayerAssignDataProvider
{

	public IEnumerable<VanillaRolePlayerAssignData> Data { get; }

	public MockVanillaRolePlayerAssignDataProvider(VanillaRolePlayerOption option)
	{
		if (option.MockOption is null)
		{
			throw new ArgumentNullException(nameof(option.MockOption));
		}


		var curPlayerData = GameData.Instance.AllPlayers.GetFastEnumerator().Select(x => new VanillaRolePlayerAssignData(x)).ToList();

		if (curPlayerData.Count > option.MockOption.PlayerNum)
		{
			this.Data = mockAssignToPlayer(curPlayerData.Take(option.MockOption.PlayerNum));
		}
		else if (curPlayerData.Count < option.MockOption.PlayerNum)
		{
			var playerNames = option.MockOption.MockPlayerNames;
			int delta = option.MockOption.PlayerNum - curPlayerData.Count;

			if (playerNames is null)
			{
				playerNames = randomName.Take(delta).ToList();
			}
			else if (playerNames.Count < delta)
			{
				var additionalNames = randomName.Take(delta - playerNames.Count).ToList();
				playerNames.AddRange(additionalNames);
			}


			byte maxPlayerId = curPlayerData.Max(x => x.PlayerId);
			this.Data = mockAssignToPlayer(curPlayerData.Concat(
				Enumerable.Range(0, option.MockOption.PlayerNum - curPlayerData.Count)
					.Select(x => new VanillaRolePlayerAssignData(
						(byte)(x + maxPlayerId + 1),
						playerNames[x], RoleTypes.Crewmate))));
		}
		else
		{
			this.Data = mockAssignToPlayer(curPlayerData);
		}
	}

	public IEnumerable<VanillaRolePlayerAssignData> mockAssignToPlayer(IEnumerable<VanillaRolePlayerAssignData> players)
	{
		var randomPlayers = players.OrderBy(x => RandomGenerator.Instance.Next()).ToList();

		var curOptions = GameOptionsManager.Instance.currentGameOptions;

		int impostorNum = curOptions.NumImpostors;
		var impostorPlayers = new List<VanillaRolePlayerAssignData>(randomPlayers.Count);
		if (impostorNum > 0)
		{
			var targetPlayers = randomPlayers.Take(impostorNum).ToList();
			impostorPlayers.AddRange(targetPlayers.Select(x => new VanillaRolePlayerAssignData(x.PlayerId, x.PlayerName, RoleTypes.Impostor)));

			foreach (var player in targetPlayers)
			{
				randomPlayers.Remove(player);
			}
		}

		if (curOptions.GameMode == GameModes.HideNSeek || 
			curOptions.GameMode == GameModes.SeekFools ||
			curOptions.RoleOptions == null)
		{
			// Assign remaining players as crewmates
			impostorPlayers.AddRange(randomPlayers);
			return impostorPlayers;
		}

		// インポスターの役職を割り当てる
		impostorPlayers = assignVanillaRoleToPlayer(
			curOptions.RoleOptions, impostorPlayers,
			RoleTypes.Shapeshifter, RoleTypes.Phantom, RoleTypes.Viper);
		// 残っているのはクルーのプレイヤーだけなので、クルーの役職を割り当てる
		var crewmatePlayers = assignVanillaRoleToPlayer(
			curOptions.RoleOptions, randomPlayers,
			RoleTypes.Scientist, RoleTypes.Engineer, RoleTypes.Detective, RoleTypes.Noisemaker, RoleTypes.Tracker);

		impostorPlayers.AddRange(crewmatePlayers);

		return impostorPlayers;
	}

	private List<VanillaRolePlayerAssignData> assignVanillaRoleToPlayer(IRoleOptionsCollection roleOptions, List<VanillaRolePlayerAssignData> players, params RoleTypes[] roles)
	{
		var result = new List<VanillaRolePlayerAssignData>(players.Count);

		foreach (var role in roles.OrderBy(x => RandomGenerator.Instance.Next()))
		{
			int roleNum = roleOptions.GetNumPerGame(role);
			int chance = roleOptions.GetChancePerGame(role);

			if (players.Count == 0)
			{
				break;
			}

			for (int i = 0; i < roleNum; i++)
			{
				if (players.Count == 0)
				{
					break;
				}

				int random = RandomGenerator.Instance.Next(0, 101);
				if (random > chance)
				{
					continue;
				}
				var targetPlayer = players[RandomGenerator.Instance.Next(players.Count)];
				result.Add(new VanillaRolePlayerAssignData(targetPlayer.PlayerId, targetPlayer.PlayerName, role));
				players.Remove(targetPlayer);
			}
		}

		result.AddRange(players);
		return result;
	}

	private static IEnumerable<string> randomName => new List<string>
	{
		"yukieiji",
		"zunda",
		"88659",
		"nanozoku",
		"exr",
		"exs",
		"exv",
		"ninchi",
		"tukuyomi",
		"yachiyo8000",
		"anko",
		"mikan",
		"impostor",
		"crewmate",
		"aoi-",
		"seyana-",
		"sorena-",
		"wakaru-",
		"arena-",
		"irop-",
		"kaguyaho-",
		"kiritanpo",
		"HelloWorld",
		"hoge",
		"sudo",
		"root",
		"AmongUS",
		"NoName",
		"名前を入れて下さい",
		"ああああ",
		"あああい",
		"あああう",
		"ExtremeRoles",
		"ExtremeSkins",
		"ExtremeHat",
		"ExtremeVisor",
		"DMZ",
	}.OrderBy(x => RandomGenerator.Instance.Next());
}

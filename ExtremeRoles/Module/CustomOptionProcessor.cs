using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using AmongUs.GameOptions;

using Twitch;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Migrator;
using ExtremeRoles.Module.CustomOption.Implemented.Old;


#nullable enable

namespace ExtremeRoles.Module;

#nullable enable

public static class CustomOptionCsvProcessor
{
	private const string csvName = "option.csv";

	private const string roleAssignModelKey = "BytedRoleAssignModel";
	private const string vanilaOptionKey = "BytedVanillaOptions";

	private const string comma = ",";
	private const int curVersion = 8;

	private sealed class StringCleaner
	{
		private Regex regex = new Regex("(\\|)|(<.*?>)|(\\\n)");

		public string Clean(string value)
			=> regex.Replace(value, string.Empty).Trim();
	}

	public static bool Export()
	{
		string path = csvName;

		if (TwitchManager.InstanceExists)
		{
			var info = WinApiHelper.SaveFile("*.csv", "Select Export FileName");
			if (info is null ||
				info.FilePath is null)
			{
				return false;
			}

			path = info.FilePath;
			if (!path.EndsWith(".csv"))
			{
				path += ".csv";
			}
		}

		ExtremeRolesPlugin.Logger.LogInfo("---------- Option Export Start ----------");

		var cleaner = new StringCleaner();

		try
		{
			using var csv = new StreamWriter(path, false, new UTF8Encoding(true));

			csv.WriteLine(
				string.Format("{1}{0}{2}{0}{3}{0}{4}",
					comma,
					"Game Infos",
					$"AmongUs ver.{UnityEngine.Application.version}",
					$"ExtremeRoles ver.{Assembly.GetExecutingAssembly().GetName().Version}",
					$"Exported on:{DateTime.UtcNow}"));

			csv.WriteLine(
				string.Format("{1}{0}{2}{0}{3}{0}{4}",
					comma, "Name", "OptionValue", "CustomOptionName", "SelectedIndex")); //ヘッダー

			foreach (var (tab, tabContainer) in OptionManager.Instance)
			{
				foreach (var cate in tabContainer.Category)
				{
					int cateId = cate.Id;
					foreach (var option in cate.Options)
					{
						var info = option.Info;
						if (PresetOption.IsPreset(cateId, info.Id))
						{
							continue;
						}

						csv.WriteLine(
							string.Format("{1}{0}{2}{0}{3}{0}{4}",
								comma,
								cleaner.Clean(option.Title),
								cleaner.Clean(option.ValueString),
								cleaner.Clean(info.Name),
								option.Selection));
					}
				}
			}

			csv.WriteLine(
				string.Format(
					"{1}{0}{1}", comma, string.Empty));

			csv.WriteLine(
				string.Format("{1}{0}{2}",
					comma,
					roleAssignModelKey,
					RoleAssignFilter.Instance.SerializeModel()));

			csv.WriteLine(
				string.Format(
					"{1}{0}{1}", comma, string.Empty));
			var gameOptionManager = GameOptionsManager.Instance;

			foreach (GameModes gameMode in Enum.GetValues<GameModes>())
			{
				IGameOptions? option = gameMode switch
				{
					GameModes.Normal or GameModes.NormalFools =>
						gameOptionManager.normalGameHostOptions.Cast<IGameOptions>(),
					GameModes.HideNSeek or GameModes.SeekFools =>
						gameOptionManager.hideNSeekGameHostOptions.Cast<IGameOptions>(),
					_ => null,
				};
				if (option == null) { continue; }
				exportIGameOptions(csv, gameOptionManager.gameOptionsFactory, option, gameMode);
			}

			ExtremeRolesPlugin.Logger.LogInfo("---------- Option Export End ----------");

			return true;
		}
		catch (Exception e)
		{
			Helper.Logging.Error(e.ToString());
		}
		return false;
	}

	public static bool Import()
	{
		string path = csvName;

		if (TwitchManager.InstanceExists)
		{
			var info = WinApiHelper.OpenFile("*.csv", "Select Import FileName");
			if (info is null ||
				info.FilePath is null)
			{
				return false;
			}

			path = info.FilePath;
			if (!path.EndsWith(".csv"))
			{
				return false;
			}
		}

		ExtremeRolesPlugin.Logger.LogInfo("---------- Option Import Start ----------");

		Dictionary<string, int> importedOption = new Dictionary<string, int>();
		Dictionary<GameModes, List<byte>> importedVanillaOptions =
			new Dictionary<GameModes, List<byte>>();

		try
		{
			using var csv = new StreamReader(path, new UTF8Encoding(true));

			if (csv is null)
			{
				return false;
			}

			string? infoData = csv.ReadLine(); // verHeader
			if (infoData is null)
			{
				return false;
			}

			string[] info = infoData.Split(comma);
			string exrVersion = info[2];

			ExtremeRolesPlugin.Logger.LogInfo(
				$"Loading from {info[1]} with {exrVersion} {info[3]} Data");

			string? line = csv.ReadLine(); // ヘッダー
			while ((line = csv.ReadLine()) != null)
			{
				string[] option = line.Split(comma);

				switch (option[0])
				{
					case "":
						continue;
					case vanilaOptionKey:
						GameModes mode = (GameModes)Enum.Parse(typeof(GameModes), option[1]);
						if (!importedVanillaOptions.TryGetValue(
								mode, out var modeOption) ||
							modeOption is null)
						{
							modeOption = new List<byte>();
							importedVanillaOptions.Add(mode, modeOption);
						}
						modeOption.Add(byte.Parse(option[2]));
						break;
					case roleAssignModelKey:
						RoleAssignFilter.Instance.DeserializeModel(option[1]);
						break;
					default:
						importedOption.Add(
							option[2], // cleanedName
							int.Parse(option[3]));
						break;
				}
			}

			var gameOptionManager = GameOptionsManager.Instance;

			foreach (var (mode, bytedOptions) in importedVanillaOptions)
			{
				IGameOptions option = gameOptionManager.gameOptionsFactory.FromBytes(
					bytedOptions.ToArray());

				if (option == null) { continue; }

				switch (mode)
				{
					case GameModes.Normal:
					case GameModes.NormalFools:
						if (!option.IsTryCast<NormalGameOption>(out var normalOption))
						{
							normalOption = gameOptionManager.MigrateNormalGameOptions(option);
						}
						gameOptionManager.normalGameHostOptions = normalOption;
						gameOptionManager.SaveNormalHostOptions();
						break;
					case GameModes.HideNSeek:
					case GameModes.SeekFools:
						if (!option.IsTryCast<HnSGameOption>(out var hideNSeekOption))
						{
							hideNSeekOption = gameOptionManager.MigrateHideNSeekGameOptions(option);
						}
						gameOptionManager.hideNSeekGameHostOptions = hideNSeekOption;
						gameOptionManager.SaveHideNSeekHostOptions();
						break;
					default:
						break;
				}
			}

			exrVersion = exrVersion.Replace("ExtremeRoles ver.", "");
			if (!Version.TryParse(exrVersion, out var version) ||
				version is null)
			{
				return false;
			}

			if (MigratorManager.IsMigrate(version))
			{
				MigratorManager.MigrateExportedOption(importedOption, version.Major);
			}

			var optionMng = OptionManager.Instance;
			var cleaner = new StringCleaner();

			foreach (var (tab, tabContainer) in optionMng)
			{
				foreach (var cate in tabContainer.Category)
				{
					int cateId = cate.Id;

					foreach (var option in cate.Options)
					{

						var optionInfo = option.Info;
						string name = optionInfo.Name;
						if (optionInfo.IsHidden ||
							PresetOption.IsPreset(cateId, optionInfo.Id) ||
							!importedOption.TryGetValue(
								cleaner.Clean(name),
							out int selection))
						{
							continue;
						}

						ExtremeRolesPlugin.Logger.LogInfo(
							$"Update Option : {name} to Selection:{selection}");

						option.Selection = selection;
					}
				}
			}
			if (AmongUsClient.Instance != null &&
				AmongUsClient.Instance.AmHost &&
				PlayerControl.LocalPlayer != null)
			{
				optionMng.ShereAllOption();// Share all selections
			}

			ExtremeRolesPlugin.Logger.LogInfo("---------- Option Import Complete ----------");

			return true;

		}
		catch (Exception newE)
		{

			ExtremeRolesPlugin.Logger.LogInfo($"Newed csv load error:{newE}");
		}
		return false;
	}

	private static void exportIGameOptions(
		StreamWriter writer,
		GameOptionsFactory factory,
		IGameOptions option, GameModes mode)
	{
		foreach (byte bytedOption in factory.ToBytes(option, false))
		{
			writer.WriteLine(
				string.Format("{1}{0}{2}{0}{3}",
					comma,
					vanilaOptionKey,
					mode.ToString(),
					bytedOption));
		}
	}
}

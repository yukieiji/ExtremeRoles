using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Compat;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Module;

#nullable enable

public enum RegionStatusEnum
{
	None,
	Ng,
	MayBeOk,
	Ok,
}

public sealed class RegionStatus(
	RegionStatusEnum statusEunm,
	DateTime time)
{
	public RegionStatusEnum Status { get; } = statusEunm;
	public DateTime Time { get; } = time;

	public bool IsUpdate()
	{
		var curTime = DateTime.UtcNow;
		return (curTime - this.Time).Hours > 1;
	}
}

public record struct Region(
	RegionStatus Status,
	IRegionInfo Info);

public static class CustomRegion
{
	public static IRegionInfo? EditableServer =>
		ServerManager.Instance.AvailableRegions.FirstOrDefault(
			x => x.Name == IRegionInfoExtension.FullCustomServerName);

	private static readonly Dictionary<string, Region> curCustomRegion = [];

	private static IRegionInfo[] newCustomRegion => [
		createStaticRegion(
			IRegionInfoExtension.ExROfficialServerTokyoManinName,
			"168.138.196.31", 22023, false), // Only ExtremeRoles!!
		createStaticRegion(
			IRegionInfoExtension.FullCustomServerName,
			ClientOption.Instance.Ip.Value,
			ClientOption.Instance.Port.Value, false),
	];

	public static bool TryGetStatus(string name, out RegionStatusEnum @enum)
	{
		@enum = RegionStatusEnum.None;
		if (!curCustomRegion.TryGetValue(name, out var region) ||
			region.Status is null)
		{
			return false;
		}
		@enum = region.Status.Status;
		return true;
	}

	public static void Add()
	{
		var serverMngr = ServerManager.Instance;
		var currentRegion = serverMngr.CurrentRegion;

		foreach (var region in newCustomRegion)
		{
			if (currentRegion != null && region.Name == currentRegion.Name)
			{
				currentRegion = region;
			}
			serverMngr.AddOrUpdateRegion(region);

			string name = region.Name;

			var status =
				curCustomRegion.TryGetValue(name, out var reg) &&
				reg.Status is not null &&
				!reg.Status.IsUpdate() ? reg.Status : getStatus(region);

			curCustomRegion[name] = new Region(status, region);
		}

		if (currentRegion == null)
		{
			return;
		}

		// 存在しないサーバーが選択されていた場合、東京サーバーを自動選択するようにする
		if (!serverMngr.AvailableRegions.Any(
				x =>
					x.Name == currentRegion.Name &&
					x.TranslateName == currentRegion.TranslateName))
		{
			currentRegion =
					curCustomRegion.TryGetValue(
						IRegionInfoExtension.ExROfficialServerTokyoManinName, out var region) &&
					region.Info != null
				 ? region.Info : serverMngr.AvailableRegions.FirstOrDefault();
		}

		if (currentRegion == null)
		{
			return;
		}

		serverMngr.SetRegion(currentRegion);
	}

	public static void UpdateEditorableRegion()
	{
		var newRegions = ServerManager.Instance.AvailableRegions.Where(
			r => !(
					r.Name == IRegionInfoExtension.ExROfficialServerTokyoManinName ||
					r.Name == IRegionInfoExtension.FullCustomServerName
				));
		ServerManager.Instance.AvailableRegions = newRegions.ToArray();
		Add();
	}

	public static void ReSelect(ServerManager instance)
	{
		var curRegion = instance.CurrentRegion;
		if (curCustomRegion.TryGetValue(curRegion.Name, out var target))
		{
			instance.CurrentRegion = target.Info;
		}
	}

	private static IRegionInfo createStaticRegion(
		string name, string ip, ushort port, bool useDtls)
	{
		var server = createServerInfo(name, ip, port, useDtls);
		if (ip.StartsWith("https"))
		{
			return new StaticHttpRegionInfo(
				name, StringNames.NoTranslation, ip, server).Cast<IRegionInfo>();
		}
		else
		{
			return new DnsRegionInfo(
				ip, name, StringNames.NoTranslation, server).Cast<IRegionInfo>();
		}
	}

	private static ServerInfo[] createServerInfo(string name, string ip, ushort port, bool useDtls)
		=> [
			new ServerInfo(name, ip, port, useDtls)
		];

	private static RegionStatus getStatus(IRegionInfo info)
	{
		var server = info.Servers.FirstOrDefault();
		if (server == null)
		{
			return defaultStatus();
		}
		try
		{
			var result = CustomServerAPI.Post($"{server.Ip}:{server.Port}").GetAwaiter().GetResult();
			if (result == null ||
				result.PostInfo == null ||
				!Enum.TryParse<RegionStatusEnum>(result.Status, out var s))
			{
				return defaultStatus();
			}

			return new RegionStatus(s, result.PostInfo.At);
		}
		catch (Exception e)
		{
			return defaultStatus(
				e is System.Text.Json.JsonException ? RegionStatusEnum.None : RegionStatusEnum.Ng);
		}
	}
	private static RegionStatus defaultStatus(RegionStatusEnum status= RegionStatusEnum.None) => new RegionStatus(
		status, DateTime.UtcNow);
}

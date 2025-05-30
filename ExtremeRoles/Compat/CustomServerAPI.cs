﻿using System;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace ExtremeRoles.Compat;

public sealed class CustomServerAPIRequest
{
	[JsonPropertyName("version")]
    public int Version { get; init; }
}

public sealed class CustomServerPostInfo
{
    [JsonPropertyName("version")]
    public int Version { get; init; }

    [JsonPropertyName("at")]
    public DateTime At { get; init; }

    public override string ToString()
        => $"from : [{Version} ({At})]";
}

public sealed class CustomServerAPIResponse
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string? Version { get; init; }

    [JsonPropertyName("post_info")]
    public CustomServerPostInfo? PostInfo { get; init; }
}

public static class CustomServerAPI
{
	public static async Task<CustomServerAPIResponse?> Post(string url)
	{
		url = url.StartsWith("http") ? url : $"http://{url}";
		var response = await ExtremeRolesPlugin.Instance.Http.PostAsJsonAsync(
			$"{url}/api/compat",
			new CustomServerAPIRequest() { Version = Constants.GetBroadcastVersion() });
		return await response.Content.ReadFromJsonAsync<CustomServerAPIResponse>();
	}
}
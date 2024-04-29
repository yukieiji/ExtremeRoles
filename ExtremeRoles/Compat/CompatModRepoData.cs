using ExtremeRoles.Module.JsonData;
using Newtonsoft.Json.Linq;

using SemanticVersioning;


#nullable enable

namespace ExtremeRoles.Compat;

internal readonly record struct CompatModRepoData(GitHubReleaseData Request, string DllName)
{
	private string tag => Request.tag_name.TrimStart('v');

	public string GetDownloadUrl()
	{
		foreach (var asset in this.Request.assets)
		{
			string? browser_download_url = asset.browser_download_url;
			if (string.IsNullOrEmpty(browser_download_url) ||
				asset.content_type.Equals("application/x-zip-compressed") ||
				!browser_download_url.EndsWith(this.DllName))
			{
				continue;
			}

			return browser_download_url;
		}
		return string.Empty;
	}
	public bool IsNewer(Version version)
	{
		if (!Version.TryParse(tag, out var myVersion)) { return false; }

		return myVersion.BaseVersion() > version.BaseVersion();
	}
}

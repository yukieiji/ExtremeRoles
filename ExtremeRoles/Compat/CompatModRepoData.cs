using Newtonsoft.Json.Linq;

using SemanticVersioning;


#nullable enable

namespace ExtremeRoles.Compat;

internal sealed record CompatModRepoData(JObject Request, string DllName)
{
				private const string contentType = "content_type";

				private string tag => Request["tag_name"].ToString().TrimStart('v');

				public string GetDownloadUrl()
				{
								JToken assets = this.Request["assets"];

								for (JToken current = assets.First; current != null; current = current.Next)
								{
												string? browser_download_url = current["browser_download_url"]?.ToString();
												if (string.IsNullOrEmpty(browser_download_url) ||
																current[contentType] == null ||
																current[contentType].ToString().Equals("application/x-zip-compressed") ||
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

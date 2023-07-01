using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using ExtremeRoles.Helper;
using Newtonsoft.Json.Linq;

namespace ExtremeRoles.Compat.Operator;

#nullable enable

internal sealed class ExRAddonInstaller : OperatorBase
{
				private string addonDll;
				private string url = "https://api.github.com/repos/yukieiji/ExtremeRoles/releases/latest";
				private const string agentName = "ExtremeRoles CompatModInstaller";

				private HttpClient client;
				private Task? installTask = null;

				internal ExRAddonInstaller(CompatModType addonType) : base()
				{
								this.addonDll = $"{addonType}.dll";

								this.client = new HttpClient();
								this.client.DefaultRequestHeaders.Add("User-Agent", agentName);
				}

				public override void Excute()
				{
								string info = Translation.GetString("checkInstallNow");
								Popup.Show(info);


								var exrRepoData = GetRestApiDataAsync(this.client, url).GetAwaiter().GetResult();

								if (exrRepoData == null)
								{
												SetPopupText(Translation.GetString("installManual"));
								}
								else
								{
												info = Translation.GetString("installNow");

												if (installTask == null)
												{
																info = Translation.GetString("installInProgress");
																installTask = downloadAndInstall(exrRepoData);
												}

												this.Popup.StartCoroutine(
																Effects.Lerp(0.01f, new Action<float>((p) => { SetPopupText(info); })));
								}
				}

				private async Task<bool> downloadAndInstall(JObject data)
				{
								JToken assets = data["assets"];

								string downloadUri = "";

								for (JToken current = assets.First; current != null; current = current.Next)
								{
												string? browser_download_url = current["browser_download_url"]?.ToString();
												if (string.IsNullOrEmpty(browser_download_url) ||
																current["content_type"] == null ||
																current["content_type"].ToString().Equals("application/x-zip-compressed") ||
																!browser_download_url.EndsWith(this.addonDll))
												{
																continue;
												}

												downloadUri = browser_download_url;
								}

								if (string.IsNullOrEmpty(downloadUri)) { return false; }

								var res = await this.client.GetAsync(downloadUri, HttpCompletionOption.ResponseContentRead);
								if (res.StatusCode != HttpStatusCode.OK || res.Content == null)
								{
												Logging.Error($"Server returned no data: {res.StatusCode}");
												return false;
								}

								string filePath = Path.Combine(this.ModFolderPath, this.addonDll);

								await using var responseStream = await res.Content.ReadAsStreamAsync();
								await using var fileStream = File.Create(filePath);
								await responseStream.CopyToAsync(fileStream);

								ShowPopup(Translation.GetString("installRestart"));

								return true;
				}
}

using System.IO;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Reflection;

using System.Threading.Tasks;

using ExtremeRoles.Extension.System.IO;

using Newtonsoft.Json.Linq;

#nullable enable

namespace ExtremeRoles.Helper;

public static class JsonParser
{
    public static JObject? GetJObjectFromAssembly(string path)
    {
		var assembly = Assembly.GetCallingAssembly();
		using Stream? stream = assembly.GetManifestResourceStream(path);

		if (stream is null) { return null; }

		int length = (int)stream.Length;
		byte[] byteArray = new byte[length];
        stream.ReadExactly(byteArray, 0, length);

        return JObject.Parse(Encoding.UTF8.GetString(byteArray));
    }

	public static T? LoadJsonStructFromAssembly<T>(string path, JsonSerializerOptions? opt=null)
	{
		var assembly = Assembly.GetCallingAssembly();
		using Stream? stream = assembly.GetManifestResourceStream(path);

		if (stream is null) { return default(T); }

		var result = JsonSerializer.Deserialize<T>(stream, opt);

		return result;
	}

	public static async Task<T?> GetRestApiAsync<T>(string targetUrl)
	{
		ExtremeRolesPlugin.Logger.LogInfo($"Conecting...:{targetUrl}");

		var response = await ExtremeRolesPlugin.Instance.Http.GetAsync(
			targetUrl, HttpCompletionOption.ResponseContentRead);
		if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
		{
			Logging.Error($"Server returned no data: {response.StatusCode}");
			return default(T);
		}

		ExtremeRolesPlugin.Logger.LogInfo("Success!!");

		ExtremeRolesPlugin.Logger.LogInfo("Deserialize to Json....");
		var stream = await response.Content.ReadAsStreamAsync();
		T? data = await JsonSerializer.DeserializeAsync<T>(stream);
		ExtremeRolesPlugin.Logger.LogInfo("Complete!!!");

		return data;
	}
}

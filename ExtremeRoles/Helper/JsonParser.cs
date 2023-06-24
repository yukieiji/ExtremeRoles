using System.IO;
using System.Text;
using System.Reflection;

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

        byte[] byteArray = new byte[stream.Length];
        stream.Read(byteArray, 0, (int)stream.Length);

        return JObject.Parse(Encoding.UTF8.GetString(byteArray));
    }
}

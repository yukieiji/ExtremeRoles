using Newtonsoft.Json.Linq;

using Il2CppInterop.Runtime.InteropTypes;

#nullable enable

namespace ExtremeRoles.Extension.Json;

public static class JsonExtension
{
    public static T? Get<T>(this JObject obj, string key) where T : Il2CppObjectBase
    {
        return obj[key].TryCast<T>();
    }

    public static T? Get<T>(this JToken token, string key) where T : Il2CppObjectBase
    {
        return token[key].TryCast<T>();
    }

    public static T? Get<T>(this JArray arr, int index) where T : Il2CppObjectBase
    {
        return arr[index].TryCast<T>();
    }

	public static bool TryGet(this JToken token, string key, out JToken? result)
	{
		result = token[key];
		return result != null;
	}
}

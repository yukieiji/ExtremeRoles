using Newtonsoft.Json.Linq;

using Il2CppInterop.Runtime.InteropTypes;

#nullable enable

namespace ExtremeRoles.Extension.Json;

public static class JsonExtension
{
    public static T? Get<T>(this JObject obj, string key) where T : Il2CppObjectBase
    {
        var token = obj[key];
        if (token is T t) return t;
        return token?.TryCast<T>();
    }

    public static T? Get<T>(this JToken token, string key) where T : Il2CppObjectBase
    {
        var child = token[key];
        if (child is T t) return t;
        return child?.TryCast<T>();
    }

    public static T? Get<T>(this JArray arr, int index) where T : Il2CppObjectBase
    {
        var item = arr[index];
        if (item is T t) return t;
        return item?.TryCast<T>();
    }

	public static bool TryGet(this JToken token, string key, out JToken? result)
	{
		result = token[key];
		return result != null;
	}
}

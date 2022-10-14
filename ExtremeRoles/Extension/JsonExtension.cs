using Newtonsoft.Json.Linq;

using UnhollowerBaseLib;

namespace ExtremeRoles.Extension.Json
{
    public static class JsonExtension
    {
        public static T Get<T>(this JObject obj, string key) where T : Il2CppObjectBase
        {
            return obj[key].TryCast<T>();
        }

        public static T Get<T>(this JToken token, string key) where T : Il2CppObjectBase
        {
            return token[key].TryCast<T>();
        }

        public static T Get<T>(this JArray arr, int index) where T : Il2CppObjectBase
        {
            return arr[index].TryCast<T>();
        }
    }
}

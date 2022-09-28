using Newtonsoft.Json.Linq;

using UnhollowerBaseLib;

namespace ExtremeRoles.Extension.Json
{
    public static class JsonExtension
    {

        public static T Get<T>(this JToken token, string key) where T : Il2CppObjectBase
        {
            return token[key].TryCast<T>();
        }

        public static T Get<T>(this JArray token, int index) where T : Il2CppObjectBase
        {
            return token[index].TryCast<T>();
        }
    }
}

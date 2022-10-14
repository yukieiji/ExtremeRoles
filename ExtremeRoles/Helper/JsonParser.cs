using System.IO;
using System.Text;
using System.Reflection;

using Newtonsoft.Json.Linq;

namespace ExtremeRoles.Helper
{
    public static class JsonParser
    {
        public static JObject GetJObjectFromAssembly(string path)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                path);
            byte[] byteArray = new byte[stream.Length];
            stream.Read(byteArray, 0, (int)stream.Length);

            return JObject.Parse(Encoding.UTF8.GetString(byteArray));
        }
    }
}

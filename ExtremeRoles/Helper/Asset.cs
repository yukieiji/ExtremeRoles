using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;

namespace ExtremeRoles.Helper
{
    public static class Asset
    {
        private static Dictionary<string, AssetBundle> loadedBundle = 
            new Dictionary<string, AssetBundle>();

        public static AssetBundle LoadAssetBundle(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                name);
            AssetBundle bundle = AssetBundle.LoadFromStream(stream.AsIl2Cpp());

            if (bundle == null)
            {
                ExtremeRolesPlugin.Logger.LogError("bundle is Null!!");
                return null;
            }
            loadedBundle.Add(name, bundle);
            return bundle;
        }

        public static GameObject GetGameObjectFromAssetBundle(
            string bundleName, string objName)
        {
            if (!loadedBundle.TryGetValue(bundleName, out AssetBundle bundle))
            {
                bundle = LoadAssetBundle(bundleName);
                if (bundle == null) { return null; }
            }
            return bundle.LoadAsset(objName).Cast<GameObject>();
        }
    }
}

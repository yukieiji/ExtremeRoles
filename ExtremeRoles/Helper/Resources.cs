using System;
using System.IO;
using System.Reflection;

using UnhollowerBaseLib;
using UnityEngine;

namespace ExtremeRoles.Helper
{
    public class Resources
    {

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;

        public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = LoadTextureFromResources(path);
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                Logging.Debug("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D LoadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                Logging.Debug("Error loading texture from resources: " + path);
            }
            return null;
        }
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
            {
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            }
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }
    }
}

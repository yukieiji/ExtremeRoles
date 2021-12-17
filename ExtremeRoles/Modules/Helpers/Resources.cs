using System;
using System.IO;
using System.Reflection;

using UnhollowerBaseLib;
using UnityEngine;

namespace ExtremeRoles.Modules.Helpers
{
    public class Resources
    {
        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromResources(string path)
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
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }
    }
}

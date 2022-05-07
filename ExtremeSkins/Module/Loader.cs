using System;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnhollowerBaseLib;

namespace ExtremeSkins.Module
{
    public static class Loader
    {
        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;

        public static Sprite CreateSpriteFromResources(
            string path, float pixelsPerUnit = 115f)
        {
            try
            {
                Texture2D texture = createTextureFromResources(path);
                return Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading sprite from path: {path}");
            }
            return null;
        }

        public static Sprite CreateSpriteFromDisk(string path)
        {
            Texture2D texture = LoadTextureFromDisk(path);
            if (texture == null)
            {
                return null;
            }
            Sprite sprite = Sprite.Create(
                texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.53f, 0.575f), texture.width * 0.375f);
            return sprite;
        }

        public static Texture2D LoadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    loadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading texture from disk: {path}");
            }
            return null;
        }

        private static Texture2D createTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                loadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading texture from resources: {path}");
            }
            return null;
        }

        private static bool loadImage(Texture2D tex, byte[] data, bool markNonReadable)
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

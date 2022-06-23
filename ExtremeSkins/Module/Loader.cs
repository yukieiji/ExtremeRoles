using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnhollowerBaseLib;

namespace ExtremeSkins.Module
{
    public static class Loader
    {
        private static Sprite titleLog = null;

        private const string titleLogPath = "ExtremeSkins.Resources.TitleBurner.png";
        private const float titlePixelPerUnit = 425f;

        public static Sprite GetTitleLog()
        {
            try
            {
                if (titleLog == null)
                {
                    Texture2D texture = createTextureFromResources(titleLogPath);
                    titleLog = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), titlePixelPerUnit);
                    titleLog.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                }

                return titleLog;
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading sprite from path: {titleLogPath}");
            }
            return null;
        }

        public static Texture2D LoadTextureFromDisk(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                byte[] byteTexture = File.ReadAllBytes(path);
                ImageConversion.LoadImage(texture, byteTexture, false);

                return texture;
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading texture from disk: {path}");
            }
            return null;
        }

        private static unsafe Texture2D createTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                long length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                var read = stream.Read(new Span<byte>(
                    IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                ExtremeSkinsPlugin.Logger.LogInfo($"Error loading texture from resources: {path}");
            }
            return null;
        }
    }
}

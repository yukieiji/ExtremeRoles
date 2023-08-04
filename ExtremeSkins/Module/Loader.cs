using System;
using System.IO;
using System.Reflection;

using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppFile = Il2CppSystem.IO.File;

using ExtremeRoles.Module;

namespace ExtremeSkins.Module;

public static class Loader
{
	public enum ErrorCode : byte
	{
		UnKnown,
		CannotFindResource,
		CannotFindImgFromDisk,
		CannotCreateTexture,
		CannotCreateSprite
	}

	public record struct LoadError(ErrorCode Code, string LoadPath)
	{
		public LoadError() : this(ErrorCode.UnKnown, "")
		{ }

		public override string ToString()
		{
			string errorMessage = Code switch
			{
				ErrorCode.CannotFindResource    => "Cannot find item in embemded resource",
				ErrorCode.CannotFindImgFromDisk => "Cannot find img in Disk",
				ErrorCode.CannotCreateTexture   => "Cannot create Texture2D",
				ErrorCode.CannotCreateSprite    => "Cannot create Sprite",
				_ => string.Empty,
			};

			return $"ErrorCode:{(int)Code},{this.Code}   {errorMessage} FromTarget:{LoadPath}";
		}
	}

    private static Sprite? titleLog = null;

    private const string titleLogPath = "ExtremeSkins.Resources.TitleBurner.png";
    private const float titlePixelPerUnit = 425f;

    public static Expected<Sprite, LoadError> GetTitleLog()
    {
        try
        {
            if (titleLog == null)
            {
                var result = createTextureFromResources(titleLogPath);

				if (!result.HasValue())
				{
					return result.Error;
				}

				var texture = result.Value;

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
			return new LoadError(ErrorCode.CannotCreateSprite, titleLogPath);
		}
    }

    public static Expected<Texture2D, LoadError> LoadTextureFromDisk(string path)
    {
        try
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            Il2CppStructArray<byte> byteTexture = Il2CppFile.ReadAllBytes(path);
            ImageConversion.LoadImage(texture, byteTexture, false);

            return texture;
        }
        catch
        {
			return new LoadError(ErrorCode.CannotFindImgFromDisk, path);
		}
	}

    private static unsafe Expected<Texture2D, LoadError> createTextureFromResources(string path)
    {
        try
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream? stream = assembly.GetManifestResourceStream(path);
            if (stream is null)
			{
				return new LoadError(ErrorCode.CannotFindResource, path);
			}
			long length = stream.Length;
            var byteTexture = new Il2CppStructArray<byte>(length);
			Span<byte> span = new Span<byte>(
				IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length);

			stream.Read(span);
            ImageConversion.LoadImage(texture, byteTexture, false);
            return texture;
        }
        catch
        {
			return new LoadError(ErrorCode.CannotCreateTexture, path);
		}
    }
}

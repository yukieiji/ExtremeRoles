using System;
using System.IO;
using System.Reflection;

using UnhollowerBaseLib;
using UnityEngine;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Resources
{

    public static class Path
    {
        public const string LangData = "ExtremeRoles.Resources.LangData.stringData.json";

        public const string BackGround = "ExtremeRoles.Resources.white.png";

        public const string TitleBurner = "ExtremeRoles.Resources.TitleBurner.png";
        public const string TabLogo = "ExtremeRoles.Resources.TabIcon.png";

        public const string MaintainerRepair = "ExtremeRoles.Resources.Repair.png";
        public const string BodyGuardShield = "ExtremeRoles.Resources.Shield.png";
        public const string TimeMasterTimeShield = "ExtremeRoles.Resources.TimeShield.png";

        public const string EvolverEvolved = "ExtremeRoles.Resources.Evolved.png";
        public const string CarrierCarry = "ExtremeRoles.Resources.Carry.png";
        public const string PainterPaint = "ExtremeRoles.Resources.Paint.png";
        public const string OverLoaderOverLoad = "ExtremeRoles.Resources.OverLoad.png";
        public const string OverLoaderDownLoad = "ExtremeRoles.Resources.DownLoad.png";
        public const string FakerDummy = "ExtremeRoles.Resources.Dummy.png";
        public const string CrackerCrack = "ExtremeRoles.Resources.Crack.png";
        public const string CrackerCrackTrace = "ExtremeRoles.Resources.CrackTrace.png";

        public const string AliceShipBroken = "ExtremeRoles.Resources.ShipBroken.png";
        public const string JackalSidekick = "ExtremeRoles.Resources.Sidekick.png";
        public const string MissionaryPropagate = "ExtremeRoles.Resources.Propagate.png";
        public const string JesterOutburst = "ExtremeRoles.Resources.Outburst.png";

        public const string TestButton = "ExtremeRoles.Resources.TESTBUTTON.png";
    }

    public static class Loader
    {

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;

        public static Sprite CreateSpriteFromResources(
            string path, float pixelsPerUnit=115f)
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
                Logging.Debug("Error loading sprite from path: " + path);
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
                Logging.Debug("Error loading texture from resources: " + path);
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

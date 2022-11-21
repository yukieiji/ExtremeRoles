using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Helper
{
    public static class Sound
    {
        public enum SoundType : byte
        {
            Kill,
            AgencyTakeTask,
            CommanderReduceKillCool,
            CurseMakerCurse,
            ReplaceNewTask,
        }

        private static Dictionary<SoundType, AudioClip> cachedAudio = 
            new Dictionary<SoundType, AudioClip>();

        private const string soundPlaceHolder = "assets/soundeffect/{0}.mp3";

        public static void RpcPlaySound(SoundType soundType, float volume=0.8f)
        {
            using (var caller = RPCOperator.CreateCaller(
                RPCOperator.Command.PlaySound))
            {
                caller.WriteByte((byte)soundType);
                caller.WriteFloat(volume);
            }
            PlaySound(soundType, volume);
        }

        public static void PlaySound(
            SoundType soundType, float volume)
        {
            AudioClip clip = getAudio(soundType);
            if (Constants.ShouldPlaySfx() && clip != null)
            {
                SoundManager.Instance.PlaySound(clip, false, volume);
            }
        }

        private static AudioClip getAudio(SoundType soundType)
        {
            if (cachedAudio.TryGetValue(soundType, out AudioClip clip))
            {
                return clip;
            }
            else
            {
                switch (soundType)
                {
                    case SoundType.Kill:
                        clip = CachedPlayerControl.LocalPlayer.PlayerControl.KillSfx;
                        break;
                    case SoundType.AgencyTakeTask:
                    case SoundType.CommanderReduceKillCool:
                    case SoundType.CurseMakerCurse:
                    case SoundType.ReplaceNewTask:
                        clip = Loader.GetUnityObjectFromResources<AudioClip>(
                            Path.SoundEffect, string.Format(
                                soundPlaceHolder, soundType.ToString()));
                        break;
                    default:
                        return null;
                }
                cachedAudio.Add(soundType, clip);
                return clip;
            }
        }
    }
}

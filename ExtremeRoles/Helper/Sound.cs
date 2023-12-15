using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Helper;

public static class Sound
{
	private const string soundPlaceHolder = "assets/soundeffect/{0}.mp3";

	public enum SoundType : byte
    {
        Kill,
        GuardianAngleGuard,

        AgencyTakeTask,
		CurseMakerCurse,

		CommanderReduceKillCool,
		TeroristSabotageAnnounce,

		MinerMineSE,

		ReplaceNewTask,
    }

    private static Dictionary<SoundType, AudioClip> cachedAudio =
        new Dictionary<SoundType, AudioClip>();

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
        AudioClip clip = GetAudio(soundType);
        if (Constants.ShouldPlaySfx() && clip != null)
        {
			ExtremeRolesPlugin.Logger.LogInfo($"Play Sound:{soundType}");
            SoundManager.Instance.PlaySound(clip, false, volume);
        }
    }

    public static AudioClip GetAudio(SoundType soundType)
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
                case SoundType.GuardianAngleGuard:
                    clip = FastDestroyableSingleton<RoleManager>.Instance.protectAnim.UseSound;
                    break;
                default:
					clip = Loader.GetUnityObjectFromResources<AudioClip>(
						Path.SoundEffect, string.Format(
							soundPlaceHolder, soundType.ToString()));
					clip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
					break;
			}
			if (!clip)
			{
				Logging.Debug("Can't load AudioClip");
			}
            cachedAudio.Add(soundType, clip);
            return clip;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Helper;

#nullable enable

public static class Sound
{
	private const string soundPlaceHolder = "assets/soundeffect/{0}.mp3";

	public enum Type : byte
    {
        Kill,
        GuardianAngleGuard,

		NullSound,

        AgencyTakeTask,
		CurseMakerCurse,

		CommanderReduceKillCool,
		TeroristSabotageAnnounce,
		ScavengerPickUpWeapon,
		ScavengerFireNormalGun,
		ScavengerFireBeam,
		BoxcerStraight,

		MinerMineSE,

		ReplaceNewTask,
    }

    private static readonly Dictionary<Type, AudioClip> cachedAudio =
        new Dictionary<Type, AudioClip>();

    public static void RpcPlaySound(Type soundType, float volume=0.8f)
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
        Type soundType, float volume)
    {
        AudioClip? clip = GetAudio(soundType);
        if (Constants.ShouldPlaySfx() && clip != null)
        {
			ExtremeRolesPlugin.Logger.LogInfo($"Play Sound:{soundType}");
            SoundManager.Instance.PlaySound(clip, false, volume);
        }
    }

    public static AudioClip? GetAudio(Type soundType)
    {
        if (cachedAudio.TryGetValue(soundType, out AudioClip? clip) &&
			clip != null)
        {
            return clip;
        }
        else
        {
            switch (soundType)
            {
                case Type.Kill:
                    clip = PlayerControl.LocalPlayer.KillSfx;
                    break;
                case Type.GuardianAngleGuard:
                    clip = RoleManager.Instance.protectAnim.UseSound;
                    break;
				case Type.TeroristSabotageAnnounce:
					clip = getRoleAudio(soundType);
					break;
                default:
					clip = UnityObjectLoader.LoadFromResources<AudioClip>(
						ObjectPath.SoundEffect, string.Format(
							soundPlaceHolder, soundType.ToString()));
					clip.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
					break;
			}
			if (clip == null)
			{
				Logging.Debug("Can't load AudioClip");
				return null;
			}
            cachedAudio[soundType] = clip;
            return clip;
        }
    }
	private static AudioClip? getRoleAudio(Type type)
	{
		ExtremeRoleId id = type switch
		{
			Type.TeroristSabotageAnnounce => ExtremeRoleId.Terorist,
			_ => ExtremeRoleId.Null
		};
		if (id is ExtremeRoleId.Null)
		{
			return null;
		}
		var result = UnityObjectLoader.LoadFromResources<AudioClip, ExtremeRoleId>(
			id, ObjectPath.GetRoleSePath(id));
		return result;
	}
}

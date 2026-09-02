using ExtremeRoles.Helper;
using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.Interface;

public interface ISoundProvider
{
	public AudioClip? GetAudio(Sound.Type soundType);
}

public sealed class DefaultSoundProvider : ISoundProvider
{
	public AudioClip? GetAudio(Sound.Type soundType)
		=> Sound.GetAudio(soundType);
}

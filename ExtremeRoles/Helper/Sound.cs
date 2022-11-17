using ExtremeRoles.Performance;

namespace ExtremeRoles.Helper
{
    public static class Sound
    {
        public enum SoundType : byte
        {
            Kill,
        }

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
            UnityEngine.AudioClip clip;
            switch (soundType)
            {
                case SoundType.Kill:
                    clip = CachedPlayerControl.LocalPlayer.PlayerControl.KillSfx;
                    break;
                default:
                    return;
            }

            if (Constants.ShouldPlaySfx() && clip != null)
            {
                SoundManager.Instance.PlaySound(clip, false, volume);
            }
        }
    }
}

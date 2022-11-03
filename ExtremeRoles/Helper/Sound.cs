using System;
using System.Collections.Generic;
using System.Text;

using Hazel;

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
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                (byte)RPCOperator.Command.PlaySound,
                Hazel.SendOption.Reliable, -1);
            writer.Write((byte)soundType);
            writer.Write(volume);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

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

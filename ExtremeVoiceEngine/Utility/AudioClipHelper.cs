using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtremeVoiceEngine.Utility;

internal class AudioClipHelper
{
    public const int BufferSize = 1024 * 32;

    public static async Task<AudioClip?> CreateFromStreamAsync(
        Stream stream, CancellationToken cancellationToken)
    {
        byte[] ftmChank = new byte[44];

        await stream.ReadAsync(ftmChank, 0, 44);
        int channels = BitConverter.ToInt16(ftmChank, 22);
        int bitPerSample = BitConverter.ToInt16(ftmChank, 34);

        if (channels != 1)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError(
                "NotSupportedException: AudioClipUtil supports only single channel.");
            return null;
        }

        if (bitPerSample != 16)
        {
            ExtremeVoiceEnginePlugin.Logger.LogError(
                "NotSupportedException: AudioClipUtil supports only 16-bit quantization.");
            return null;
        }

        int bytePerSample = bitPerSample / 8;
        int frequency = BitConverter.ToInt32(ftmChank, 24);
        int length = BitConverter.ToInt32(ftmChank, 40);

        AudioClip? audioClip = null;
        try
        {
            audioClip = AudioClip.Create("AudioClip", length / bytePerSample, channels, frequency, false);

            byte[] readBuffer = new byte[BufferSize];
            float[] samplesBuffer = new float[BufferSize / bytePerSample];
            int samplesOffset = 0;
            int readBytes;

            while ((readBytes = await stream.ReadAsync(
                readBuffer, 0, readBuffer.Length, cancellationToken)) > 0)
            {
                if (readBytes % 2 != 0)
                {
                    // If an odd number of bytes were read, read an additional 1 byte
                    await stream.ReadAsync(readBuffer, readBytes, 1, cancellationToken);
                    readBytes++;
                }

                int readSamples = readBytes / bytePerSample;

                // Supports only 16-bit quantization, and single channel.
                for (int i = 0; i < readSamples; i++)
                {
                    short value = BitConverter.ToInt16(readBuffer, i * bytePerSample);
                    samplesBuffer[i] = value / 32768f;
                }

                if (readBytes == BufferSize)
                {
                    audioClip.SetData(samplesBuffer, samplesOffset);
                }
                else
                {
                    var segment = new ArraySegment<float>(samplesBuffer, 0, readSamples);
                    audioClip.SetData(segment.ToArray(), samplesOffset);
                }
                samplesOffset += readSamples;
            }
        }
        catch (Exception e)
        {
            if (audioClip != null)
            {
                UnityEngine.Object.Destroy(audioClip);
            }
            string exceptionMessage = e.Message;
            string message = e is OperationCanceledException ?
                exceptionMessage : 
                $"IOException : AudioClipUtil: WAV data decode failed.  {exceptionMessage}";
            ExtremeVoiceEnginePlugin.Logger.LogError(message);
            return null;
        }

        return audioClip;
    }
}
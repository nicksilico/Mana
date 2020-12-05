using System;
using System.Numerics;
using OpenTK.Audio.OpenAL;

namespace Mana.Audio.OpenAL
{
    public class ALSound : Sound
    {
        public int BufferHandle { get; }

        public WaveAudio WaveAudio { get; }

        private bool _disposed;

        internal ALSound(WaveAudio waveAudio)
        {
            WaveAudio = waveAudio;

            BufferHandle = AL.GenBuffer();
            ALHelper.CheckLastError();

            AL.BufferData(BufferHandle,
                          ALHelper.GetSoundFormat(WaveAudio.WaveInfo.Channels, WaveAudio.WaveInfo.BitDepth),
                          ref WaveAudio.WaveData[0],
                          WaveAudio.WaveData.Length,
                          WaveAudio.WaveInfo.SampleRate);
            ALHelper.CheckLastError();

            Duration = WaveAudio.Duration;
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                AL.DeleteBuffer(BufferHandle);
                ALHelper.CheckLastError();

                _disposed = true;
            }
        }

        public override void Play(float volume = 1,
                                  float pitch = 1,
                                  bool looping = false,
                                  Vector3 position = new Vector3())
        {
            ALSource source = OpenALBackend.ALInstance.RentSource();

            if (source == null)
                throw new InvalidOperationException();

            source.Sound = this;
            source.Looping = looping;
            source.Volume = volume;
            source.Pitch = pitch;
            source.Position = position;

            source.Play();
        }
    }
}

using System;
using System.Numerics;
using OpenTK.Audio.OpenAL;

namespace Mana.Audio.OpenAL
{
    public class ALSource
    {
        private int _sourceHandle;

        private ALSound _sound = null;
        private float _volume = 1.0f;
        private float _pitch = 1.0f;
        private bool _looping = false;
        private Vector3 _position = Vector3.Zero;

        public ALSource()
        {
            _sourceHandle = AL.GenSource();
            ALHelper.CheckLastError();

            Initialize();
        }

        public ALSound Sound
        {
            get => _sound;
            set
            {
                if (_sound == value)
                    return;

                AL.Source(_sourceHandle, ALSourcei.Buffer, value?.BufferHandle ?? 0);
                ALHelper.CheckLastError();

                _sound = value;
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_volume == value)
                    return;

                AL.Source(_sourceHandle, ALSourcef.Gain, value);
                ALHelper.CheckLastError();

                _volume = value;
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_pitch == value)
                    return;

                AL.Source(_sourceHandle, ALSourcef.Pitch, value);
                ALHelper.CheckLastError();

                _pitch = value;
            }
        }

        public bool Looping
        {
            get => _looping;
            set
            {
                if (_looping == value)
                    return;

                AL.Source(_sourceHandle, ALSourceb.Looping, value);
                ALHelper.CheckLastError();

                _looping = value;
            }
        }

        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;

                AL.Source(_sourceHandle, ALSource3f.Position, value.X, value.Y, value.Z);
                ALHelper.CheckLastError();

                _position = value;
            }
        }

        public void Initialize()
        {
            if (Sound != null)
                Stop();

            Sound = null;
            Volume = 1.0f;
            Pitch = 1.0f;
            Looping = false;
            Position = Vector3.Zero;
        }

        public void Play()
        {
            if (_sourceHandle == -1)
                throw new InvalidOperationException();

            AL.SourcePlay(_sourceHandle);
            ALHelper.CheckLastError();
        }

        public void Stop()
        {
            if (_sourceHandle == -1)
                throw new InvalidOperationException();

            AL.SourceStop(_sourceHandle);
            ALHelper.CheckLastError();
        }

        public void Pause()
        {
            if (_sourceHandle == -1)
                throw new InvalidOperationException();

            AL.SourcePause(_sourceHandle);
            ALHelper.CheckLastError();
        }
    }
}

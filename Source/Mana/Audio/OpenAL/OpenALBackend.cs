using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Mana.Audio.OpenAL
{
    public class OpenALBackend : AudioBackend
    {
        public static OpenALBackend ALInstance { get; private set; }

        private ALContext _context;
        private ALSourcePool _sourcePool;

        public OpenALBackend()
        {
            if (ALInstance != null)
                throw new InvalidOperationException("Only one ALInstance can exist at a time.");

            ALInstance = this;

            var audioDevice = ALC.OpenDevice(null);
            _context = ALC.CreateContext(audioDevice, new int[] { });
            ALC.MakeContextCurrent(_context);
            
            _sourcePool = new ALSourcePool();
        }

        public override Sound CreateSound(WaveAudio waveAudio)
        {
            return new ALSound(waveAudio);
        }

        public override void Dispose()
        {
            ALC.DestroyContext(_context);
        }

        public ALSource RentSource() => _sourcePool.Rent();
    }
}

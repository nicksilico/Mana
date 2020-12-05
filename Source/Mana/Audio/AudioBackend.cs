using System;
using Mana.Graphics;

namespace Mana.Audio
{
    public abstract class AudioBackend : IGameSystem, IDisposable
    {
        public static AudioBackend Instance { get; private set; }

        protected AudioBackend()
        {
            if (Instance != null)
                throw new InvalidOperationException("Only one AudioBackend instance may exist.");

            Instance = this;
        }

        public abstract Sound CreateSound(WaveAudio waveAudio);

        public abstract void Dispose();

        public virtual void OnAddedToGame(Game game)
        {
        }

        public virtual void EarlyUpdate(float time, float deltaTime)
        {
        }

        public virtual void LateUpdate(float time, float deltaTime)
        {
        }

        public virtual void EarlyRender(float time, float deltaTime, RenderContext renderContext)
        {
        }

        public virtual void LateRender(float time, float deltaTime, RenderContext renderContext)
        {
        }
    }
}

using System;
using System.Numerics;
using Mana.Asset;

namespace Mana.Audio
{
    public abstract class Sound : IAsset, IDisposable
    {
        public abstract void Dispose();

        public double Duration { get; protected set; }
        public string SourcePath { get; set; }
        public AssetManager AssetManager { get; set; }

        public abstract void Play(float volume = 1, float pitch = 1, bool looping = false, Vector3 position = new Vector3());
    }
}

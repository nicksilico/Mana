using System;
using System.IO;
using Mana.Audio;
using Mana.Graphics;

namespace Mana.Asset.Loaders
{
    public class SoundLoader : IAssetLoader<Sound>
    {
        public Sound Load(AssetManager assetManager, RenderContext renderContext, Stream stream, string sourcePath)
        {
            if (AudioBackend.Instance == null)
                throw new InvalidOperationException("Cannot load a Sound without first initializing an audio backend.");

            return AudioBackend.Instance.CreateSound(WaveAudio.LoadFromStream(stream));
        }
    }
}
using System.Numerics;
using ImGuiNET;
using Mana.Audio;
using Mana.Graphics;

namespace Mana.Example
{
    public class AudioGame : ExampleGame
    {
        private float _volume = 1.0f;
        private float _pitch = 1.0f;
        private bool _looping = false;
        private Vector3 _position = Vector3.Zero;

        private bool _musicPlaying = false;

        private Sound _musicSound;
        private Sound _explosionSound;

        public override void Initialize()
        {
            base.Initialize();

            _musicSound = AssetManager.Load<Sound>("./Sounds/bensound-littleidea.wav");
            _explosionSound = AssetManager.Load<Sound>("./Sounds/Explosion.wav");
        }

        public override void Update(float time, float deltaTime)
        {
        }

        public override void Render(float time, float deltaTime)
        {
            RenderContext.Clear(Color.Chocolate);

            ImGui.Begin("Audio Controls");

            ImGui.DragFloat("Volume", ref _volume);
            ImGui.DragFloat("Pitch", ref _pitch);
            ImGui.Checkbox("Looping", ref _looping);
            ImGui.DragFloat3("Position", ref _position);

            ImGui.Separator();

            if (!_musicPlaying && ImGui.Button("Play looping music"))
            {
                _musicPlaying = true;
                _musicSound.Play(looping: true);
            }

            if (ImGui.Button("Play explosion sound"))
            {
                _explosionSound.Play(_volume, _pitch, _looping, _position);
            }

            ImGui.End();
        }
    }
}
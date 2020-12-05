using System;
using System.Numerics;
using ImGuiNET;
using Mana.Graphics;
using Mana.Graphics.Shaders;
using Mana.IMGUI;
using OpenTK.Mathematics;
using Color = Mana.Graphics.Color;
using Vector2 = System.Numerics.Vector2;

namespace Mana.Example
{
    public class ExampleGame : Game
    {
        private ShaderProgram _spriteShader;
        private ShaderProgram _lineShader;
        private LineBatch2D _lineBatch;

        private int _iterations = 10;
        private bool _progressTime = true;

        private float _currentTime = 0f;

        private Color _backgroundColor = Color.CornflowerBlue;

        public override void Initialize()
        {
            AddGameSystem(new ImGuiSystem());

            _spriteShader = BasicShaderFactory.CreateSpriteShaderProgram(RenderContext);
            _lineShader = BasicShaderFactory.CreateLineShaderProgram(RenderContext);

            _lineBatch = new LineBatch2D(RenderContext) { ShaderProgram = _lineShader };
        }

        public override void Update(float time, float deltaTime)
        {
            if (_progressTime)
                _currentTime += deltaTime;

            _iterations += Input.MouseWheelDelta;
            _iterations = Math.Clamp(_iterations, 1, int.MaxValue);
        }

        public override void Render(float time, float deltaTime)
        {
            _spriteShader.SetUniform("projection", Matrix4x4.CreateOrthographicOffCenter(0f, Window.Width, Window.Height, 0f, -1f, 1f));
            _lineShader.SetUniform("projection", Matrix4x4.CreateOrthographicOffCenter(0f, Window.Width, Window.Height, 0f, -1f, 1f));

            RenderContext.Clear(_backgroundColor);

            _lineBatch.Begin();

            Vector2 getPoint(float input, Vector2 center, float radius)
            {
                float x1 = MathHelper.Lerp(center.X - radius, center.X + radius, (MathF.Sin((input * MathHelper.Pi * 2)) + 1.0f) / 2f);
                float y1 = MathHelper.Lerp(center.Y - radius, center.Y + radius, (MathF.Cos((input * MathHelper.Pi * 2)) + 1.0f) / 2f);

                return new Vector2(x1, y1);
            }

            Color getColor(float input)
            {
                return Color.FromHSV(input, 1.0f, 1.0f);
            }

            void DrawRing(int iterations, float radius)
            {
                var center = new Vector2(300, 300);

                float inc = 1.0f / iterations;

                for (float factor = 0; factor < 1.0f; factor += inc)
                {
                    _lineBatch.DrawLine(getPoint(factor + (_currentTime / 4f), center, radius), getColor(factor * 360f),
                                        getPoint((factor + inc) + (_currentTime / 4f), center, radius), getColor((factor + inc) * 360f));
                }
            }

            for (float i = 0; i < 100; i += 100 / (float)_iterations)
            {
                DrawRing(_iterations, i);
            }

            _lineBatch.End();

            ImGui.Begin("Controls");

            ImGui.DragInt("Iterations", ref _iterations, 1f, 1, int.MaxValue);

            ImGui.Checkbox("Progress Time", ref _progressTime);

            var c = _backgroundColor.ToVector3();
            ImGui.ColorPicker3("Background Color", ref c);
            _backgroundColor = Color.FromVector3(c);

            ImGui.End();
        }
    }
}
using System;
using ImGuiNET;
using Mana.Graphics;
using Mana.Graphics.Shaders;
using Mana.Graphics.Text;
using Mana.IMGUI;
using OpenTK.Windowing.Common;
using Color = Mana.Graphics.Color;
using Vector2 = System.Numerics.Vector2;

namespace Mana.Example
{
    public class FontTestingGame : ExampleGame
    {
        private Font _roboto;

        private IntPtr _textureHandle;

        private SpriteBatch _spriteBatch;
        private ShaderProgram _shaderProgram;

        private int _height = 48;
        private float _scale = 1f;
        private string _str = "The quick brown fox jumps over the lazy dog.";

        private Color _textColor = Color.White;
        private Color _backgroundColor = Color.Gray;

        public FontTestingGame()
            : base(CreateInitializationParameters())
        {
        }

        private static InitializationParameters CreateInitializationParameters()
        {
            var init = InitializationParameters.Default;

            init.OpenGLMajorVersion = 4;
            init.OpenGLMinorVersion = 6;
            init.VSync = VSyncMode.Off;

            return init;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            _roboto = new Font(RenderContext, "./Assets/Fonts/Roboto-Medium.ttf", 48);

            _textureHandle = ImGuiHelper.BindTexture(_roboto.FontAtlas);

            _shaderProgram = AssetManager.Load<ShaderProgram>("./Shaders/text.json", true);

            _spriteBatch = new SpriteBatch(RenderContext);

            Window.Maximize();
        }

        public override void Update(float time, float deltaTime)
        {
        }

        public override void Render(float time, float deltaTime)
        {
            _shaderProgram.SetUniform("projection", ref Window.ProjectionMatrix);

            RenderContext.Clear(_backgroundColor);

            _spriteBatch.Begin(_shaderProgram);

            _spriteBatch.DrawString(_roboto, _str, new Vector2(100, 100), _textColor, _scale);

            _spriteBatch.End();

            ImGui.Begin("Font--");

            ImGui.InputInt("Height", ref _height);
            ImGui.InputText("Display String", ref _str, 128);
            ImGui.DragFloat("Text Scale", ref _scale);

            ImGui.Separator();

            ImGuiHelper.ColorEdit4("Text Color", ref _textColor);
            ImGuiHelper.ColorEdit4("Background Color", ref _backgroundColor);

            ImGui.Separator();

            if (ImGui.Button("Reload"))
            {
                ImGuiHelper.UnbindTexture(_textureHandle);
                _roboto.Dispose();
                _roboto = new Font(RenderContext, "./Assets/Fonts/Roboto-Medium.ttf", (uint)_height);
                _textureHandle = ImGuiHelper.BindTexture(_roboto.FontAtlas);
            }

            ImGui.Separator();
            ImGuiHelper.Image(_textureHandle, new Vector2(_roboto.FontAtlas.Width, _roboto.FontAtlas.Height));

            ImGui.End();

            ImGui.ShowMetricsWindow();
        }
    }
}

using System.Drawing;
using ImGuiNET;
using Mana.Graphics;
using Mana.Graphics.Shaders;
using Mana.Graphics.Sprite;
using Mana.Graphics.Textures;
using Color = Mana.Graphics.Color;

namespace Mana.Example
{
    public class SpriteTestingGame : ExampleGame
    {
        private SpriteBatch _spriteBatch;
        private ShaderProgram _spriteShader;

        private Texture2D _tilesetTexture;
        private Tileset _tileset;

        #region

        public override void Initialize()
        {
            base.Initialize();

            _tilesetTexture = AssetManager.Load<Texture2D>("./Assets/Textures/tilesheet.png");
            TextureHelper.SetParametersForPixelArt(RenderContext, _tilesetTexture);

            _tileset = Tileset.FromTileSize(_tilesetTexture, 16);

            _spriteShader = BasicShaderFactory.CreateSpriteShaderProgram(RenderContext);
            _spriteBatch = new SpriteBatch(RenderContext);
        }

        public override void Update(float time, float deltaTime)
        {
        }

        #endregion

        public override void Render(float time, float deltaTime)
        {
            _spriteShader.SetUniform("projection", ref Window.ProjectionMatrix);

            RenderContext.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(_spriteShader);

            const int COUNT = 16;
            const int WIDTH = 64;

            for (int x = 0; x < COUNT; x++)
            {
                for (int y = 0; y < COUNT; y++)
                {
                    _spriteBatch.Draw(_tileset, x, y, new Rectangle(x * WIDTH, y * WIDTH, WIDTH, WIDTH), Color.White);
                }
            }

            _spriteBatch.End();

            ImGui.Begin("Controls");

            ImGui.End();

            ImGui.ShowMetricsWindow();
        }
    }
}
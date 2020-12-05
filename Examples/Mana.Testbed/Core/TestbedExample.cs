using System;
using Mana.Asset;
using Mana.Graphics;

namespace Mana.Testbed.Core
{
    public abstract class TestbedExample
    {
        private Game _game;
        private ManaWindow _window;

        public abstract string Name { get; }
        public ManaWindow Window => _window;
        public AssetManager AssetManager { get; private set; }
        public RenderContext RenderContext => _game.RenderContext;

        protected TestbedExample(Game game)
        {
            _game = game;
            _window = game.Window as ManaWindow;
        }

        public void InitializeImpl()
        {
            AssetManager = new AssetManager(_game, _game.RenderContext);
            
#if DEBUG
            AssetManager.RootPath = "../../../Assets";
#else
            AssetManager.RootPath = "./Assets";
#endif
            
            Initialize();
        }

        public void Unload()
        {
            AssetManager.Dispose();
        }

        protected abstract void Initialize();

        public abstract void Update(float time, float deltaTime);
        public abstract void Render(float time, float deltaTime);
    }
}
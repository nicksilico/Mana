using Mana.IMGUI;
using Mana.Utilities;

namespace Mana.Example
{
    public class ExampleGame : Game
    {
        public ExampleGame()
        {
        }

        public ExampleGame(InitializationParameters initializationParameters)
            : base(initializationParameters)
        {
        }

        public override void Initialize()
        {
            AddGameSystem(new ImGuiSystem());

#if DEBUG
            ConsoleHelper.SetWindowPosition(-1280, 0, 1280, 720);
            AssetManager.RootPath = "../../../Assets";
#else
            AssetManager.RootPath = "./Assets";
#endif
        }

        public override void Update(float time, float deltaTime) { }

        public override void Render(float time, float deltaTime) { }
    }
}
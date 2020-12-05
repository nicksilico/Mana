using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mana
{
    public static class KeyboardStateExtensions
    {
        public static bool WasKeyPressed(this KeyboardState state, Keys key)
        {
            return state.IsKeyDown(key) && !state.WasKeyDown(key);
        }

        public static bool WasKeyReleased(this KeyboardState state, Keys key)
        {
            return !state.IsKeyDown(key) && state.WasKeyDown(key);
        }
    }
}
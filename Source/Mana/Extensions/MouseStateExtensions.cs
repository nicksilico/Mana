using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Mana
{
    public static class MouseStateExtensions
    {
        public static bool WasMouseButtonPressed(this MouseState state, MouseButton button)
        {
            return state.IsButtonDown(button) && !state.WasButtonDown(button);
        }
        
        public static bool WasMouseButtonReleased(this MouseState state, MouseButton button)
        {
            return !state.IsButtonDown(button) && state.WasButtonDown(button);
        }
    }
}
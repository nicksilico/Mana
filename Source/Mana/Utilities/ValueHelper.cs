using System.Runtime.CompilerServices;

namespace Mana.Utilities
{
    public static class ValueHelper
    {
        /// <summary>
        /// Swaps the values of two given objects using a temporary local variable.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
    }
}

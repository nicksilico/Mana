using System;

namespace Mana.Graphics.Shaders
{
    /// <summary>
    /// The exception that is thrown when Mana encounters an error during shader pre-processing.
    /// </summary>
    public class ShaderPreprocessException : Exception
    {
        public ShaderPreprocessException(string message)
            : base(message.TrimEnd('\n'))
        {
        }
    }
}
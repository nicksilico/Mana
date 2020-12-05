using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mana.Utilities
{
    public static class ImageHelper
    {
        public static Span<Rgba32> GetPixelSpan(Image<Rgba32> image)
        {
            if (!image.TryGetSinglePixelSpan(out var span))
            {
                throw new InvalidOperationException("Could not get Span<Rgba32> from Image<Rgba32>. " +
                                                    "Is the image invalid?");
            }
            
            return span;
        }
    }
}
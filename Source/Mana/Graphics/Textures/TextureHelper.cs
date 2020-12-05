using OpenTK.Graphics.OpenGL4;

namespace Mana.Graphics.Textures
{
    public static class TextureHelper
    {
        public static void SetParametersForPixelArt(RenderContext renderContext, Texture2D texture)
        {
            texture.SetWrapModeS(renderContext, TextureWrapMode.ClampToEdge);
            texture.SetWrapModeT(renderContext, TextureWrapMode.ClampToEdge);
            texture.SetMinFilter(renderContext, TextureMinFilter.Nearest);
            texture.SetMagFilter(renderContext, TextureMagFilter.Nearest);
        }

        public static void SetParametersForPixelArt(RenderContext renderContext, Texture2DArray textureArray)
        {
            textureArray.SetWrapModeS(renderContext, TextureWrapMode.ClampToEdge);
            textureArray.SetWrapModeT(renderContext, TextureWrapMode.ClampToEdge);
            textureArray.SetMinFilter(renderContext, TextureMinFilter.Nearest);
            textureArray.SetMagFilter(renderContext, TextureMagFilter.Nearest);
        }
    }
}
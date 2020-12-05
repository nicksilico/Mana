using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Mana.Graphics.Textures;
using SharpFont;

namespace Mana.Graphics.Text
{
    public class
        Font : IDisposable
    {
        private const int COUNT = 128;
        private const int PADDING = 1;

        private static Library _library = new Library();

        public Texture2D FontAtlas { get; }

        public int Height { get; set; }

        public readonly Dictionary<char, FontCharacter> Characters;

        public Font(RenderContext renderContext, string path, uint height)
        {
            Height = (int)height;

            var face = new Face(_library, path);

            face.SetPixelSizes(0, height);

            var glyphs = GetGlyphData(face);

            int atlasSize = -1;

            for (int i = 0; i < 20; i++)
            {
                int currentSize = (int)Math.Pow(2, i);

                if (GenerateTextureAtlasData(glyphs, currentSize))
                {
                    atlasSize = currentSize;
                    break;
                }
            }

            var texture = Texture2D.CreateEmpty(renderContext, atlasSize, atlasSize);

            for (int i = 0; i < COUNT; i++)
            {
                if (glyphs[i].BufferData != null)
                {
                    texture.SetDataFromAlphaByteArray(renderContext,
                                                      glyphs[i].Bounds.X,
                                                      glyphs[i].Bounds.Y,
                                                      glyphs[i].Size.X,
                                                      glyphs[i].Size.Y,
                                                      glyphs[i].BufferData);
                }
            }

            FontAtlasHelper.FlipFontAtlas(renderContext, texture);

            var characters = new Dictionary<char, FontCharacter>();

            for (int i = 0; i < glyphs.Length; i++)
            {
                var glyph = glyphs[i];

                characters.Add(glyph.Character, new FontCharacter
                {
                    Visible = glyph.BufferData != null,
                    Bearing = glyph.Bearing,
                    Advance = glyph.Advance,
                    Bounds = glyph.Bounds,
                });

            }

            face.Dispose();

            Characters = characters;
            FontAtlas = texture;
        }

        private static bool GenerateTextureAtlasData(GlyphInfo[] glyphs, int size)
        {
            int currentLineMaxHeight = 0;

            int cursorTop = 0;
            int cursorLeft = 0;

            for (int i = 0; i < glyphs.Length; i++)
            {
                var glyph = glyphs[i];

                // If it's out of bounds to the right, try going to the next line.
                if (cursorLeft + glyph.Size.X + (PADDING * 2) > size)
                {
                    // Advance line
                    cursorTop += currentLineMaxHeight;
                    cursorLeft = 0;

                    // We created a new line.
                    currentLineMaxHeight = 0;
                }

                // If it's out of bounds to the bottom, we're out of space.
                if (cursorTop + glyph.Size.Y + (PADDING * 2) >= size)
                    return false;

                // Either it's not out of bounds to the right, or it was and we successfully created a new line.

                int thisGlyphLeftPosition = cursorLeft;
                int thisGlyphTopPosition = cursorTop;

                glyphs[i].Bounds = new Rectangle(thisGlyphLeftPosition + 1, thisGlyphTopPosition + 1, glyph.Size.X, glyph.Size.Y);

                cursorLeft = cursorLeft + glyph.Size.X + (PADDING * 2);

                if ((glyph.Size.Y + (PADDING * 2)) > currentLineMaxHeight)
                {
                    currentLineMaxHeight = (glyph.Size.Y + (PADDING * 2));
                }
            }

            return true;
        }

        private static GlyphInfo[] GetGlyphData(Face face)
        {
            var glyphs = new GlyphInfo[COUNT];

            for (uint i = 0; i < COUNT; i++)
            {
                face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                var bitmap = face.Glyph.Bitmap;

                if (bitmap.Width <= 0 || bitmap.Rows <= 0)
                {

                    glyphs[i].BufferData = null;
                    glyphs[i].Bearing = new Point(face.Glyph.BitmapLeft, face.Glyph.BitmapTop);
                    glyphs[i].Advance = face.Glyph.Advance.X.Value;
                }
                else
                {
                    glyphs[i].BufferData = face.Glyph.Bitmap.BufferData.ToArray();
                    glyphs[i].Size = new Point(bitmap.Width, bitmap.Rows);
                    glyphs[i].Bearing = new Point(face.Glyph.BitmapLeft, face.Glyph.BitmapTop);
                    glyphs[i].Advance = face.Glyph.Advance.X.Value;
                }

                glyphs[i].Character = (char)i;
            }

            // Sort by height and return.
            return glyphs.OrderByDescending(x => x.Size.Y).ToArray();
        }

        private struct GlyphInfo
        {
            public char Character;
            public byte[] BufferData;
            public Point Size;
            public Point Bearing;
            public int Advance;
            public Rectangle Bounds;
        }

        public Rectangle GetTextBounds(ReadOnlySpan<char> text, Point location)
        {
            int left = location.X;
            int bottom = location.Y;
            int top = int.MaxValue;
            int right = int.MinValue;

            int cursorX = left;
            int cursorY = bottom;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < text.Length; i++)
            {
                if (!Characters.TryGetValue(text[i], out var character))
                    continue;

                int characterLeft = cursorX + (character.Bearing.X);
                int characterTop = cursorY - (character.Bearing.Y);

                int characterBottom = characterTop + character.Bounds.Height;
                int characterRight = characterLeft + character.Bounds.Height;

                if (characterBottom > bottom)
                    bottom = characterBottom;

                if (characterTop < top)
                    top = characterTop;

                right = characterRight;

                cursorX += character.Advance >> 6;
            }

            return new Rectangle(left, top, right - left, bottom - top);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle GetTextBounds(ReadOnlySpan<char> text, Vector2 location)
        {
            return GetTextBounds(text, location.ToPoint());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rectangle GetTextBounds(ReadOnlySpan<char> text)
        {
            return GetTextBounds(text, Point.Empty);
        }

        public void Dispose()
        {
            FontAtlas?.Dispose();
        }
    }

    public struct FontCharacter
    {
        public bool Visible;
        public Point Bearing;
        public int Advance;
        public Rectangle Bounds;
    }
}

using System;
using System.Drawing;
using Mana.Graphics.Textures;

namespace Mana.Graphics.Sprite
{
    public class Tileset
    {
        private readonly int _tileCountHorizontal;
        private readonly int _tileCountVertical;
        private readonly int _tileSizeHorizontal;
        private readonly int _tileSizeVertical;

        public Tileset(Texture2D texture, int tileCountHorizontal, int tileCountVertical)
        {
            Texture2D = texture;

            _tileCountHorizontal = tileCountHorizontal;
            _tileCountVertical = tileCountVertical;

            _tileSizeHorizontal = texture.Width / tileCountHorizontal;
            _tileSizeVertical = texture.Height / tileCountVertical;
        }

        public Tileset(Texture2D texture, int tileCount)
            : this(texture, tileCount, tileCount)
        {
        }

        public static Tileset FromTileSize(Texture2D texture, int tileSizeHorizontal, int tileSizeVertical)
        {
            return new Tileset(texture, texture.Width / tileSizeHorizontal, texture.Width / tileSizeVertical);
        }

        public static Tileset FromTileSize(Texture2D texture, int tileSize)
        {
            return Tileset.FromTileSize(texture, tileSize, tileSize);
        }

        public Texture2D Texture2D { get; }

        public int TileCountHorizontal => _tileCountHorizontal;
        public int TileCountVertical => _tileCountVertical;

        public int TileSizeHorizontal => _tileSizeHorizontal;
        public int TileSizeVertical => _tileSizeVertical;

        public Rectangle GetTileRegion(int x, int y)
        {
            if (x < 0 || x >= _tileCountHorizontal)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= _tileCountVertical)
                throw new ArgumentOutOfRangeException(nameof(y));

            return new Rectangle(x * _tileSizeHorizontal,
                                 y * _tileSizeVertical,
                                 _tileSizeHorizontal,
                                 _tileSizeVertical);
        }
    }
}

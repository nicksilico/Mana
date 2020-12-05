using System;

namespace Mana.Utilities
{
    public class LineReader : IDisposable
    {
        private string _s;
        private int _pos;
        private int _length;
        private bool _finished = false;

        public LineReader(string str)
        {
            _s = str;
            _length = str.Length;
        }

        public bool Finished => _finished;

        public ReadOnlyMemory<char> ReadLine()
        {
            if (_s == null)
            {
                throw new ObjectDisposedException(nameof(_s));
            }

            int i = _pos;
            while (i < _length)
            {
                char ch = _s[i];
                if (ch == '\r' || ch == '\n')
                {
                    ReadOnlyMemory<char> result = _s.AsMemory(_pos, i - _pos);
                    _pos = i + 1;
                    if (ch == '\r' && _pos < _length && _s[_pos] == '\n')
                    {
                        _pos++;
                    }

                    return result;
                }

                i++;
            }

            if (i > _pos)
            {
                ReadOnlyMemory<char> result = _s.AsMemory(_pos, i - _pos);
                _pos = i;
                return result;
            }

            _finished = true;
            return null;
        }

        public void Dispose()
        {
            _s = null;
            _pos = 0;
            _length = 0;
        }
    }
}

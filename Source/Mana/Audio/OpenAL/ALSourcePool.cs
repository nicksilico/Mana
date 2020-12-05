﻿using System;

namespace Mana.Audio.OpenAL
{
    public class ALSourcePool
    {
        public const uint POOL_SIZE = 256;

        private ALSource[] _instances;
        private int _current = 0;

        public ALSourcePool()
        {
            _instances = new ALSource[POOL_SIZE];

            for (int i = 0; i < POOL_SIZE; i++)
                _instances[i] = new ALSource();
        }

        /// <summary>
        /// Attempts to rent an ALSource object from the source pool, using a given
        /// ALSound object to determine the priority.
        /// </summary>
        public ALSource Rent()
        {
            ALSource source = null;

            // TODO: Add distance and/or volume checks to cull sounds that are furthest away rather than
            // just picking the oldest non-looping one.

            for (int i = _current; i < _current + POOL_SIZE; i++)
            {
                var currentSource = _instances[i % POOL_SIZE];

                if (!currentSource.Looping)
                {
                    source = currentSource;
                    _current = (int)(i % POOL_SIZE);
                    break;
                }
            }

            if (source == null)
            {
                _current++;
            }

            if (_current >= _instances.Length)
            {
                _current %= _instances.Length;
            }

            source?.Initialize();
            return source;
        }
    }
}

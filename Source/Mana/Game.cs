using System;
using System.Collections.Generic;
using System.Drawing;
using Mana.Asset;
using Mana.Graphics;
using Mana.Utilities;
using Mana.Utilities.Threading;

namespace Mana
{
    public abstract class Game : IDisposable
    {
        private static readonly Logger _log = Logger.Create();
        
        private readonly List<IGameSystem> _gameSystems = new List<IGameSystem>(4);

        private readonly InitializationParameters _initializationParameters;

        protected Game(InitializationParameters initializationParameters)
        {
            _initializationParameters = initializationParameters;
        }

        protected Game()
        {
            _initializationParameters = InitializationParameters.Default;
        }

        public IGameHost Window { get; private set; }

        public RenderContext RenderContext { get; private set; }

        public AssetManager AssetManager { get; private set; }

        public Dispatcher EarlyUpdateDispatcher { get; } = new Dispatcher();

        public bool Disposed { get; private set; }

        public abstract void Initialize();

        public abstract void Update(float time, float deltaTime);

        public abstract void Render(float time, float deltaTime);

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            Dispose(true);
            GC.SuppressFinalize(this);

            Disposed = true;
            AssetManager.Dispose();
        }

        /// <summary>
        /// Creates a <see cref="ManaWindow"/> and runs the game within it.
        /// </summary>
        public void Run()
        {
            using var window = new ManaWindow(_initializationParameters);

            Window = window;
            
            window.Run(this);
        }

        public void AddGameSystem(IGameSystem gameSystem)
        {
            _gameSystems.Add(gameSystem);
            gameSystem.OnAddedToGame(this);
        }

        public void Quit()
        {
            Window.Close();
        }

        internal void UpdateBase(float time, float deltaTime)
        {
            AssetManager.Update();

            EarlyUpdateDispatcher.ProcessActionQueue();

            foreach (var system in _gameSystems)
            {
                system.EarlyUpdate(time, deltaTime);
            }

            Update(time, deltaTime);

            foreach (var system in _gameSystems)
            {
                system.LateUpdate(time, deltaTime);
            }
        }

        internal void RenderBase(float time, float deltaTime)
        {
            foreach (var system in _gameSystems)
            {
                system.EarlyRender(time, deltaTime, RenderContext);
            }

            Render(time, deltaTime);

            foreach (var system in _gameSystems)
            {
                system.LateRender(time, deltaTime, RenderContext);
            }
        }

        /// <summary>
        /// Prepares the game to be hosted by the given IGameHost. This should not be called manually unless you are
        /// creating your own IGameHost; it is automatically called when running a game with a ManaWindow.
        /// </summary>
        public void OnBeforeRun(IGameHost host)
        {
            if (Disposed)
                throw new InvalidOperationException("Cannot call OnBeforeRun on a disposed Game.");

            if (host == null)
                throw new ArgumentNullException(nameof(host));

            RenderContext = host.RenderContext ?? throw new ArgumentException("host's RenderContext may not be null", nameof(host));

            AssetManager = new AssetManager(this, RenderContext);

            Initialize();

            RenderContext.ViewportRectangle = new Rectangle(0, 0, host.Width, host.Height);
        }

        protected virtual void Dispose(bool disposing)
        {
            AssetManager.Dispose();
        }
    }
}

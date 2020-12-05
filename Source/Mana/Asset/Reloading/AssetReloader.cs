using System.Collections.Generic;
using Mana.Utilities.Threading;

namespace Mana.Asset.Reloading
{
    internal class AssetReloader
    {
        public readonly AssetManager AssetManager;
        public readonly Dispatcher Dispatcher;

        private Dictionary<string, IAssetWatcher> _watchers;

        public AssetReloader(AssetManager assetManager)
        {
            AssetManager = assetManager;
            Dispatcher = new Dispatcher();

            _watchers = new Dictionary<string, IAssetWatcher>();
        }

        public void Update()
        {
            Dispatcher.ProcessActionQueue();
        }

        public void AddReloadable(IReloadableAsset reloadable)
        {
            _watchers.Add(reloadable.SourcePath, new AssetWatcher(AssetManager, reloadable));
        }

        public void RemoveReloadable(IReloadableAsset reloadable)
        {
            if (_watchers.TryGetValue(reloadable.SourcePath, out var watcher))
            {
                _watchers.Remove(reloadable.SourcePath);
                watcher.Dispose();
            }
        }
    }
}
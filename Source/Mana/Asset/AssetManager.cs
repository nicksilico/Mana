using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mana.Asset.Loaders;
using Mana.Asset.Reloading;
using Mana.Audio;
using Mana.Graphics;
using Mana.Graphics.Geometry;
using Mana.Graphics.Shaders;
using Mana.Graphics.Textures;
using Mana.Utilities.Algorithm;

namespace Mana.Asset
{
    /// <summary>
    /// Represents an AssetManager capable of loading and managing game assets.
    /// </summary>
    public class AssetManager : IDisposable
    {
        public static readonly Dictionary<Type, IAssetLoader> AssetLoaders = new Dictionary<Type, IAssetLoader>
        {
            [typeof(Texture2D)]       = new Texture2DLoader(),
            [typeof(TextureCubeMap)]  = new TextureCubeMapLoader(),
            [typeof(Texture2DArray)]  = new Texture2DArrayLoader(),
            [typeof(ShaderProgram)]   = new ShaderProgramLoader(),
            [typeof(Model)]           = new ModelLoader(),
            [typeof(Sound)]           = new SoundLoader(),
        };

        internal readonly object AssetLock = new object();
        internal readonly AssetReloader AssetReloader;

        private LockedDictionary<string, IAsset> _assetCache = new LockedDictionary<string, IAsset>();
        private List<IDisposable> _loadedAssets = new List<IDisposable>();

        public AssetManager(Game game, RenderContext renderContext)
        {
            Game = game;

            RenderContext = renderContext;

            AssetReloader = new AssetReloader(this);
        }

        public Game Game { get; }

        public bool Disposed { get; private set; }

        public string RootPath { get; set; } = "";

        /// <summary>
        /// The main <see cref="RenderContext"/> used by the AssetManager.
        /// </summary>
        public RenderContext RenderContext { get; }

        public void Dispose()
        {
            if (Disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);

            Disposed = true;
        }

        /// <summary>
        /// Loads an asset of the given type from the given path.
        /// </summary>
        /// <typeparam name="TAsset">The <see cref="Type"/> of the asset to load.</typeparam>
        /// <param name="path">The path to the given asset.</param>
        /// <param name="liveReload">Whether the asset will be reloaded upon being changed.</param>
        /// <returns>The loaded asset.</returns>
        [DebuggerStepThrough]
        public TAsset Load<TAsset>(string path, bool liveReload = false)
            where TAsset : IAsset
        {
            if (Disposed)
                throw new InvalidOperationException("Cannot call Load on a disposed AssetManager.");

            path = Path.GetFullPath(Path.Combine(RootPath, path));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            path = path.Replace('\\', '/')
                       .Replace('/', Path.DirectorySeparatorChar);

            if (_assetCache.TryGetValue(path, out var cachedAsset))
            {
                if (!(cachedAsset is TAsset castCached))
                    throw new ArgumentException($"Cached asset of type \"{cachedAsset.GetType().Name}\" cannot be " +
                                                $"loaded as type \"{typeof(TAsset).FullName}\".");
                return castCached;
            }

            if (!AssetLoaders.TryGetValue(typeof(TAsset), out IAssetLoader loader))
                throw new ArgumentException("AssetManager does not contain a loader for type " +
                                            $"\"{typeof(TAsset).FullName}\".");

            if (!(loader is IAssetLoader<TAsset> typedLoader))
                throw new Exception($"Invalid registered loader for type \"{typeof(TAsset).FullName}\".");

            var stream = GetStreamFromPath(path);

            TAsset asset = typedLoader.Load(this,
                                            RenderContext,
                                            stream,
                                            path);

            asset.SourcePath = path;
            asset.AssetManager = this;

            _loadedAssets.Add(asset);
            _assetCache.Add(path, asset);

            OnAssetLoaded(asset, liveReload);

            return asset;
        }

       /// <summary>
       /// Adds an existing <see cref="IDisposable"/> object to the <see cref="AssetManager"/> so that it will be
       /// disposed when the <see cref="AssetManager"/> is disposed. 
       /// </summary>
       /// <returns>
       /// Returns true if the <see cref="IDisposable"/> was successfully added to the <see cref="AssetManager"/>, or
       /// false if the <see cref="AssetManager"/> already contains the <see cref="IDisposable"/>.
       /// </returns>
        public bool AddDisposable(IDisposable asset)
        {
            if (_loadedAssets.Contains(asset))
            {
                return false;
            }
            
            _loadedAssets.Add(asset);
            return true;
        }

        public Stream GetStreamFromPath(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"No file was found at the path: \"{path}\"");

            return File.OpenRead(path);
        }

        public void Unload(IAsset asset)
        {
            if (Disposed)
                throw new InvalidOperationException("Cannot call Unload on a disposed AssetManager.");

            if (!_assetCache.Remove(asset.SourcePath))
                throw new ArgumentException("Asset was not found in AssetManager. This will occur if the asset's " +
                                            "SourcePath value is changed manually.");

            if (asset is IReloadableAsset reloadableAsset)
                AssetReloader.RemoveReloadable(reloadableAsset);

            OnAssetUnloading(asset);

            _loadedAssets.Remove(asset);
            asset.Dispose();
        }

        public void Update()
        {
            if (Disposed)
                throw new InvalidOperationException("Cannot call Update on a disposed AssetManager.");

            AssetReloader.Update();
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (IDisposable loadedAsset in _loadedAssets.ToArray())
            {
                if (_loadedAssets is IAsset asset && asset.AssetManager == this)
                {
                    Unload(asset);                    
                }
                else
                {
                    loadedAsset.Dispose();
                }
            }

            _assetCache.Clear();
        }

        private void OnAssetLoaded(IAsset asset, bool liveReload)
        {
            asset.OnAssetLoaded();

            if (liveReload)
            {
                if (!(asset is IReloadableAsset reloadableAsset))
                    throw new InvalidOperationException($"Asset of type {asset.GetType().Name} is not reloadable.");

                AssetReloader.AddReloadable(reloadableAsset);
            }
        }

        private void OnAssetUnloading(IAsset asset)
        {
            if (asset is IReloadableAsset reloadable)
                AssetReloader.RemoveReloadable(reloadable);
        }
    }
}

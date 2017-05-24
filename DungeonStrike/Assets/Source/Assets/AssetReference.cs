namespace DungeonStrike.Source.Assets
{
    /// <summary>
    /// Stores information needed to retrieve an asset from an asset bundle.
    /// </summary>
    public struct AssetReference
    {
        public AssetReference(string bundleName, string assetName)
        {
            BundleName = bundleName;
            AssetName = assetName;
        }

        /// <summary>
        /// Name of AssetBundle in which the asset is stored.
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// Name of the asset file on disk without file extension.
        /// </summary>
        public string AssetName { get; }
    }
}
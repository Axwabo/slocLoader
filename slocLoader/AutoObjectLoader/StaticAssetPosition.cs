using Axwabo.Helpers.Config;

namespace slocLoader.AutoObjectLoader;

[Serializable]
public struct StaticAssetPosition : IAssetLocation
{

    public string AssetName { get; set; }

    public StaticMapPoint Point { get; set; }

    public IMapPoint Location() => Point;

    public bool MakeObjectsStatic { get; set; }

    public StaticAssetPosition(string assetName, StaticMapPoint location)
    {
        AssetName = assetName;
        Point = location;
        MakeObjectsStatic = true;
    }

}

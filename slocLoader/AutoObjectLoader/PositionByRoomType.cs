using Axwabo.Helpers.Config;

namespace slocLoader.AutoObjectLoader;

[Serializable]
public struct PositionByRoomType : IAssetLocation
{

    public string AssetName { get; set; }

    public MapPointByRoomType Point { get; set; }

    public IMapPoint Location() => Point;

    public bool MakeObjectsStatic { get; set; }

    public PositionByRoomType(string assetName, MapPointByRoomType location)
    {
        AssetName = assetName;
        Point = location;
        MakeObjectsStatic = false;
    }

}

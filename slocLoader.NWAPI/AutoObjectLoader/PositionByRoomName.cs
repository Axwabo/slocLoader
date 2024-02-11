using Axwabo.Helpers.Config;

namespace slocLoader.AutoObjectLoader;

[Serializable]
public struct PositionByRoomName : IAssetLocation
{

    public string AssetName { get; set; }

    public MapPointByName Point { get; set; }

    public IMapPoint Location() => Point;

    public bool MakeObjectsStatic { get; set; }

    public PositionByRoomName(string assetName, MapPointByName location)
    {
        AssetName = assetName;
        Point = location;
        MakeObjectsStatic = false;
    }

}

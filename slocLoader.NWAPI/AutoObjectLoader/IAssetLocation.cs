using Axwabo.Helpers.Config;

namespace slocLoader.AutoObjectLoader;

public interface IAssetLocation
{

    public string AssetName { get; set; }

    public IMapPoint Location();

    public bool MakeObjectsStatic { get; }

}

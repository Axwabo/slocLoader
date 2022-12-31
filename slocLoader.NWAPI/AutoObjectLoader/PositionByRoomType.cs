using System;
using Axwabo.Helpers.Config;

namespace slocLoader.AutoObjectLoader {

    [Serializable]
    public struct PositionByRoomType : IAssetLocation {

        public string AssetName { get; set; }

        public MapPointByRoomType Point { get; set; }

        public IMapPoint Location() => Point;

        public PositionByRoomType(string assetName, MapPointByRoomType location) {
            AssetName = assetName;
            Point = location;
        }

    }

}

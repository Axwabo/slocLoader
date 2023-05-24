using System.ComponentModel;
using Axwabo.Helpers.Config;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using slocLoader.AutoObjectLoader;

namespace slocLoader;

public sealed class slocConfig : IConfig
{

    public bool IsEnabled { get; set; } = true;

    public bool Debug { get; set; } = false;

    [Description("If objects in AppData should be automatically loaded.")]
    public bool AutoLoad { get; set; } = true;

    [Description("Enables or disables automatic spawning of objects specified in the config.")]
    public bool EnableAutoSpawn { get; set; } = true;

    [Description("A list of objects to be automatically spawned specified by room names.")]
    public List<PositionByRoomName> AutoSpawnByRoomName { get; set; } = new()
    {
        new PositionByRoomName("example", new MapPointByName("none", Vector3.forward, new SerializedRotation(10)))
    };

    [Description("A list of objects to be automatically spawned specified by EXILED RoomTypes.")]
    public List<PositionByRoomType> AutoSpawnByRoomType { get; set; } = new()
    {
        new PositionByRoomType("example", new MapPointByRoomType(RoomType.Unknown, Vector3.up, new SerializedRotation(0, 30)))
    };

    [Description("A list of objects to be automatically spawned at specific locations.")]
    public List<StaticAssetPosition> AutoSpawnByLocation { get; set; } = new()
    {
        new StaticAssetPosition("example", new StaticMapPoint(0, 1000, 5))
    };

    [Description("If set to false, teleporters will not be able to modify the positions of ragdolls. When enabled, can cause issues with networking and client-side ragdoll physics.")]
    public bool EnableRagdollPositionModification { get; set; } = true;

}

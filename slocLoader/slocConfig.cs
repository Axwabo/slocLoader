using System.Collections.Generic;
using System.ComponentModel;
using Axwabo.Helpers.Config;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using slocLoader.AutoObjectLoader;
using UnityEngine;

namespace slocLoader {

    public class Config : IConfig {

        public bool IsEnabled { get; set; } = true;

        [Description("If objects in AppData should be automatically loaded.")]
        public bool AutoLoad { get; set; } = true;

        [Description("Enables or disables automatic spawning of objects specified in the config.")]
        public bool EnableAutoSpawn { get; set; } = true;

        [Description("A list of objects to be automatically spawned specified by room names.")]
        public List<PositionByRoomName> AutoSpawnByRoomName { get; set; } = new() {
            new PositionByRoomName("example", new MapPointByName("none", Vector3.forward, new SerializedRotation(10)))
        };

        [Description("A list of objects to be automatically spawned specified by EXILED RoomTypes.")]
        public List<PositionByRoomType> AutoSpawnByRoomType { get; set; } = new() {
            new PositionByRoomType("example", new MapPointByRoomType(RoomType.Unknown, Vector3.up, new SerializedRotation(0, 30)))
        };

    }

}

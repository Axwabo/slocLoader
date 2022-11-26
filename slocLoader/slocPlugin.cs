using System;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using MapGeneration;
using slocLoader.AutoObjectLoader;

namespace slocLoader {

    public class slocPlugin : Plugin<Config> {

        public override string Name => "slocLoader";
        public override string Prefix => "sloc";
        public override string Author => "Axwabo";
        public override Version Version { get; } = new(2, 0, 0);
        public override Version RequiredExiledVersion { get; } = new(5, 2, 0);

        private Harmony _harmony;

        public override void OnEnabled() {
            _harmony = new Harmony("Axwabo.slocLoader");
            try {
                _harmony.PatchAll();
                Log.Info("Patching succeeded.");
            } catch (Exception e) {
                Log.Error("Patching failed! Nested object scaling will behave weirdly!\n" + e);
            }

            API.UnsetPrefabs();
            if (Config.AutoLoad)
                API.PrefabsLoaded += AutomaticObjectLoader.LoadObjects;
            if (SeedSynchronizer.MapGenerated) {
                API.LoadPrefabs();
                SpawnDefault();
            }

            Exiled.Events.Handlers.Map.Generated += API.LoadPrefabs;
            API.PrefabsLoaded += SpawnDefault;
            base.OnEnabled();
        }

        public override void OnDisabled() {
            _harmony.UnpatchAll();
            API.UnsetPrefabs();
            Exiled.Events.Handlers.Map.Generated -= API.LoadPrefabs;
            API.PrefabsLoaded -= AutomaticObjectLoader.LoadObjects;
            API.PrefabsLoaded -= SpawnDefault;
            base.OnDisabled();
        }

        private void SpawnDefault() {
            if (Config.EnableAutoSpawn)
                AutomaticObjectLoader.SpawnObjects(Config.AutoSpawnByRoomName.Cast<IAssetLocation>().Concat(Config.AutoSpawnByRoomType.Cast<IAssetLocation>()).Concat(Config.AutoSpawnByLocation.Cast<IAssetLocation>()));
        }

    }

}

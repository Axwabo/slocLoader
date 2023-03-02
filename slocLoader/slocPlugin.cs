using System;
using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using MapGeneration;
using slocLoader.AutoObjectLoader;

namespace slocLoader {

    public sealed class slocPlugin : Plugin<slocConfig> {

        public override string Name => "slocLoader";
        public override string Prefix => "sloc";
        public override string Author => "Axwabo";
        public override Version Version { get; } = new(4, 0, 0);
        public override Version RequiredExiledVersion { get; } = new(6, 0, 0);

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
            Exiled.Events.Handlers.Map.Generated += API.LoadPrefabs;
            API.PrefabsLoaded += SpawnDefault;
            if (SeedSynchronizer.MapGenerated)
                API.LoadPrefabs();
            base.OnEnabled();
        }

        public override void OnDisabled() {
            _harmony.UnpatchAll();
            API.UnsetPrefabs();
            Exiled.Events.Handlers.Map.Generated -= API.LoadPrefabs;
            API.PrefabsLoaded -= SpawnDefault;
            base.OnDisabled();
        }

        private void SpawnDefault() {
            if (Config.EnableAutoSpawn)
                AutomaticObjectLoader.SpawnObjects(
                    Config.AutoSpawnByRoomName.Cast<IAssetLocation>()
                        .Concat(Config.AutoSpawnByRoomType.Cast<IAssetLocation>())
                        .Concat(Config.AutoSpawnByLocation.Cast<IAssetLocation>())
                );
        }

    }

}

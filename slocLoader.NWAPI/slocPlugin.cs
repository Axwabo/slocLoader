using System;
using System.Linq;
using HarmonyLib;
using MapGeneration;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using slocLoader.AutoObjectLoader;

namespace slocLoader {

    public sealed class slocPlugin {

        internal static slocPlugin Instance;

        private Harmony _harmony;

        [PluginConfig]
        public slocConfig Config = new();

        [PluginEntryPoint("slocLoader", "4.0.0", "A plugin that loads sloc files.", "Axwabo")]
        public void OnEnabled() {
            Instance = this;
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

            SeedSynchronizer.OnMapGenerated += API.LoadPrefabs;
            API.PrefabsLoaded += SpawnDefault;
            Log.Info("slocLoader has been enabled");
        }

        [PluginUnload]
        public void OnDisabled() {
            _harmony.UnpatchAll();
            API.UnsetPrefabs();
            API.PrefabsLoaded -= AutomaticObjectLoader.LoadObjects;
            API.PrefabsLoaded -= SpawnDefault;
            SeedSynchronizer.OnMapGenerated -= API.LoadPrefabs;
            Log.Info("slocLoader has been disabled");
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

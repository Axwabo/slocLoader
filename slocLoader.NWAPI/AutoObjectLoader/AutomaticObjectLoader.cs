using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using slocLoader.Objects;

namespace slocLoader.AutoObjectLoader {

    public static class AutomaticObjectLoader {

        private static readonly Dictionary<string, List<slocGameObject>> LoadedObjects = new();

        public static void LoadObjects() {
            LoadedObjects.Clear();
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED", "Plugins", "sloc", "Objects");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var loaded = 0;
            foreach (var file in Directory.EnumerateFiles(path, "*.sloc")) {
                var name = Path.GetFileNameWithoutExtension(file);
                try {
                    LoadedObjects.Add(name, API.ReadObjectsFromFile(file));
                    loaded++;
                } catch (Exception e) {
                    Log.Warn($"Failed to parse object \"{name}\":\n{e}");
                }
            }

            Log.Debug($"Loaded {loaded} object{(loaded == 1 ? "" : "s")} from AppData.");
        }

        public static void SpawnObjects(IEnumerable<IAssetLocation> locations) {
            var totalAssetsToSpawn = 0;
            var spawnedAssets = 0;
            var spawnedObjects = 0;
            foreach (var name in locations) {
                totalAssetsToSpawn++;
                if (!TryGetObjects(name.AssetName, true, out var objects))
                    continue;
                var pose = name.Location().WorldPose();
                API.SpawnObjects(objects, out var spawned, pose.position, pose.rotation);
                spawnedAssets++;
                spawnedObjects += spawned;
            }

            Log.Debug($"Spawned {spawnedAssets} asset(s) out of {totalAssetsToSpawn} specified; {spawnedObjects} object(s) in total.");
        }

        public static bool TryGetObjects(string name, out List<slocGameObject> objects) => LoadedObjects.TryGetValue(name, out objects);

        public static bool TryGetObjects(string name, bool ignoreCase, out List<slocGameObject> objects) {
            if (!ignoreCase)
                return LoadedObjects.TryGetValue(name, out objects);
            objects = LoadedObjects.FirstOrDefault(e => e.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
            return objects != null;
        }

    }

}

﻿using slocLoader.ObjectCreation;
using slocLoader.Objects;

namespace slocLoader.AutoObjectLoader;

public static class AutomaticObjectLoader
{

    private static readonly Dictionary<string, List<slocGameObject>> LoadedObjects = new();

    public static void LoadObjects()
    {
        LoadedObjects.Clear();
#if EXILED
        var path = Path.Combine(Paths.Plugins, "sloc", "Objects");
#else
        var path = Path.Combine(PluginHandler.Get(slocPlugin.Instance).PluginDirectoryPath, "Objects");
#endif
        Directory.CreateDirectory(path);
        var loaded = 0;
        foreach (var file in Directory.EnumerateFiles(path, "*.sloc"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            try
            {
                LoadedObjects.Add(name, API.ReadObjectsFromFile(file));
                loaded++;
            }
            catch (Exception e)
            {
#if EXILED
                Log.Warn($"Failed to parse object \"{name}\":\n{e}");
#else
                Log.Warning($"Failed to parse object \"{name}\":\n{e}");
#endif
            }
        }

        Log.Info($"Loaded {loaded} object{(loaded == 1 ? "" : "s")} from AppData.");
    }

    public static void SpawnObjects(IEnumerable<IAssetLocation> locations)
    {
        var totalAssetsToSpawn = 0;
        var spawnedAssets = 0;
        var spawnedObjects = 0;
        foreach (var asset in locations)
        {
            totalAssetsToSpawn++;
            if (!TryGetObjects(asset.AssetName, true, out var objects))
                continue;
            var pose = asset.Location().WorldPose();
            API.SpawnObjects(objects, new CreateOptions
            {
                Position = pose.position,
                Rotation = pose.rotation,
                IsStatic = asset.MakeObjectsStatic
            }, out var spawned);
            spawnedAssets++;
            spawnedObjects += spawned;
        }

        Log.Info($"Spawned {spawnedAssets} asset(s) out of {totalAssetsToSpawn} specified; {spawnedObjects} object(s) in total.");
    }

    public static bool TryGetObjects(string name, out List<slocGameObject> objects) => LoadedObjects.TryGetValue(name, out objects);

    public static bool TryGetObjects(string name, bool ignoreCase, out List<slocGameObject> objects)
    {
        if (!ignoreCase)
            return LoadedObjects.TryGetValue(name, out objects);
        objects = LoadedObjects.FirstOrDefault(e => e.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
        return objects != null;
    }

}

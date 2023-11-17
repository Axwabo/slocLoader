using AdminToys;
using HarmonyLib;
using MapGeneration;
using PluginAPI.Core.Attributes;
using slocLoader.AutoObjectLoader;

namespace slocLoader;

public sealed class slocPlugin
{

    internal static slocPlugin Instance;

    private Harmony _harmony;

    [PluginConfig]
    public slocConfig Config = new();

    [PluginEntryPoint("slocLoader", "5.0.0", "A plugin that loads sloc files.", "Axwabo")]
    public void OnEnabled()
    {
        Instance = this;
        _harmony = new Harmony("Axwabo.slocLoader");
        try
        {
            _harmony.PatchAll();
            RemoveMapEditorRebornPatch();
            Log.Info("Patching succeeded.");
        }
        catch (Exception e)
        {
            Log.Error("Patching failed! Nested object scaling will behave weirdly!\n" + e);
        }

        if (Config.AutoLoad)
            try
            {
                AutomaticObjectLoader.LoadObjects();
            }
            catch (Exception e)
            {
                Log.Error("Failed to load objects automatically:\n" + e);
            }

        API.UnsetPrefabs();
        SeedSynchronizer.OnMapGenerated += API.LoadPrefabs;
        API.PrefabsLoaded += SpawnDefault;
        if (SeedSynchronizer.MapGenerated)
            API.LoadPrefabs();
        Log.Info("slocLoader has been enabled");
    }

    [PluginUnload]
    public void OnDisabled()
    {
        _harmony.UnpatchAll();
        API.UnsetPrefabs();
        API.PrefabsLoaded -= SpawnDefault;
        SeedSynchronizer.OnMapGenerated -= API.LoadPrefabs;
        Log.Info("slocLoader has been disabled");
    }

    private static void RemoveMapEditorRebornPatch()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name == "MapEditorReborn");
        if (assembly == null)
            return;
        var patch = AccessTools.Method("MapEditorReborn.Patches.UpdatePositionServerPatch:Prefix");
        if (patch == null)
        {
            Log.Warning("Failed to find MapEditorReborn.Patches.UpdatePositionServerPatch type! Nested object scaling will behave weirdly!");
            return;
        }

        var instance = AccessTools.Property("MapEditorReborn.MapEditorReborn:Singleton")?.GetValue(null);
        if (instance == null)
        {
            Log.Warning("Failed to find MapEditorReborn.MapEditorReborn.Singleton property! Nested object scaling will behave weirdly!");
            return;
        }

        if (AccessTools.Field("MapEditorReborn.MapEditorReborn:_harmony")?.GetValue(instance) is not Harmony harmony)
        {
            Log.Warning("Failed to find MapEditorReborn.MapEditorReborn._harmony field! Nested object scaling will behave weirdly!");
            return;
        }

        harmony.Unpatch(AccessTools.Method(typeof(AdminToyBase), nameof(AdminToyBase.UpdatePositionServer)), patch);
        Log.Info("Removed MapEditorReborn patch.");
    }

    private void SpawnDefault()
    {
        if (Config.EnableAutoSpawn)
            AutomaticObjectLoader.SpawnObjects(
                (Config.AutoSpawnByRoomName?.Cast<IAssetLocation>()).AsNonNullEnumerable()
                .Concat((Config.AutoSpawnByRoomType?.Cast<IAssetLocation>()).AsNonNullEnumerable())
                .Concat((Config.AutoSpawnByLocation?.Cast<IAssetLocation>()).AsNonNullEnumerable())
            );
    }

}

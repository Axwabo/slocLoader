using AdminToys;
using Exiled.API.Enums;
using HarmonyLib;
using MapGeneration;
using slocLoader.AutoObjectLoader;
using Map = Exiled.Events.Handlers.Map;

namespace slocLoader;

public sealed class slocPlugin : Plugin<slocConfig>
{

    internal static slocPlugin Instance;

    public override string Name => "slocLoader";
    public override string Prefix => "sloc";
    public override string Author => "Axwabo";
    public override PluginPriority Priority => PluginPriority.Lower;
    public override Version Version { get; } = new(4, 2, 2);
    public override Version RequiredExiledVersion { get; } = new(8, 3, 0);

    private Harmony _harmony;

    public override void OnEnabled()
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
        Map.Generated += API.LoadPrefabs;
        API.PrefabsLoaded += SpawnDefault;
        if (SeedSynchronizer.MapGenerated)
            API.LoadPrefabs();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        _harmony.UnpatchAll();
        API.UnsetPrefabs();
        Map.Generated -= API.LoadPrefabs;
        API.PrefabsLoaded -= SpawnDefault;
        base.OnDisabled();
    }

    private static void RemoveMapEditorRebornPatch()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name == "MapEditorReborn");
        if (assembly == null)
            return;
        var patch = AccessTools.Method("MapEditorReborn.Patches.UpdatePositionServerPatch:Prefix");
        if (patch == null)
        {
            Log.Warn("Failed to find MapEditorReborn.Patches.UpdatePositionServerPatch type! Nested object scaling will behave weirdly!");
            return;
        }

        var instance = AccessTools.Property("MapEditorReborn.MapEditorReborn:Singleton")?.GetValue(null);
        if (instance == null)
        {
            Log.Warn("Failed to find MapEditorReborn.MapEditorReborn.Singleton property! Nested object scaling will behave weirdly!");
            return;
        }

        if (AccessTools.Field("MapEditorReborn.MapEditorReborn:_harmony")?.GetValue(instance) is not Harmony harmony)
        {
            Log.Warn("Failed to find MapEditorReborn.MapEditorReborn._harmony field! Nested object scaling will behave weirdly!");
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

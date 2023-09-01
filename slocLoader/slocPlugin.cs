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
    public override Version Version { get; } = new(4, 1, 3);
    public override Version RequiredExiledVersion { get; } = new(7, 0, 0);

    private Harmony _harmony;

    public override void OnEnabled()
    {
        Instance = this;
        _harmony = new Harmony("Axwabo.slocLoader");
        try
        {
            _harmony.PatchAll();
            Log.Info("Patching succeeded.");
        }
        catch (Exception e)
        {
            Log.Error("Patching failed! Nested object scaling will behave weirdly!\n" + e);
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

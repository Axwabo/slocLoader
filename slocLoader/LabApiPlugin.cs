using AdminToys;
using HarmonyLib;
using LabApi.Events.Handlers;
using LabApi.Loader.Features.Plugins;
using MapGeneration;
using slocLoader.AutoObjectLoader;

namespace slocLoader;

public sealed class slocPlugin : Plugin<slocConfig>
{

    internal static slocPlugin Instance;

    public override string Name => "slocLoader";
    public override string Description { get; }
    public override string Author { get; }
    public override Version Version { get; }
    public override Version RequiredApiVersion { get; }

    private Harmony _harmony;

    public override void Enable()
    {
        Instance = this;
        _harmony = new Harmony("Axwabo.slocLoader");
        try
        {
            _harmony.PatchAll();
            RemoveMapEditorRebornPatch();
            Logger.Info("Patching succeeded.");
        }
        catch (Exception e)
        {
            Logger.Error("Patching failed! Nested object scaling will behave weirdly!\n" + e);
        }

        if (Config?.AutoLoad ?? true)
            try
            {
                AutomaticObjectLoader.LoadObjects();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load objects automatically:\n" + e);
            }

        API.UnsetPrefabs();
        API.PrefabsLoaded += SpawnDefault;
        ServerEvents.WaitingForPlayers += API.LoadPrefabs;
        if (SeedSynchronizer.MapGenerated)
            API.LoadPrefabs();
        Logger.Info("slocLoader has been enabled");
    }

    public override void Disable()
    {
        _harmony.UnpatchAll(_harmony.Id);
        API.UnsetPrefabs();
        API.PrefabsLoaded -= SpawnDefault;
        ServerEvents.WaitingForPlayers -= API.LoadPrefabs;
        Logger.Info("slocLoader has been disabled");
    }

    private static void RemoveMapEditorRebornPatch()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(e => e.GetName().Name == "MapEditorReborn");
        if (assembly == null)
            return;
        var patch = AccessTools.Method("MapEditorReborn.Patches.UpdatePositionServerPatch:Prefix");
        if (patch == null)
        {
            Logger.Warn("Failed to find MapEditorReborn.Patches.UpdatePositionServerPatch type! Nested object scaling will behave weirdly!");
            return;
        }

        var instance = AccessTools.Property("MapEditorReborn.MapEditorReborn:Singleton")?.GetValue(null);
        if (instance == null)
        {
            Logger.Warn("Failed to find MapEditorReborn.MapEditorReborn.Singleton property! Nested object scaling will behave weirdly!");
            return;
        }

        if (AccessTools.Field("MapEditorReborn.MapEditorReborn:_harmony")?.GetValue(instance) is not Harmony harmony)
        {
            Logger.Warn("Failed to find MapEditorReborn.MapEditorReborn._harmony field! Nested object scaling will behave weirdly!");
            return;
        }

        harmony.Unpatch(AccessTools.Method(typeof(AdminToyBase), nameof(AdminToyBase.UpdatePositionServer)), patch);
        Logger.Info("Removed MapEditorReborn patch.");
    }

    private void SpawnDefault()
    {
        if (Config is {EnableAutoSpawn: true})
            AutomaticObjectLoader.SpawnObjects(
                (Config.AutoSpawnByRoomName?.Cast<IAssetLocation>()).AsNonNullEnumerable()
                .Concat((Config.AutoSpawnByRoomType?.Cast<IAssetLocation>()).AsNonNullEnumerable())
                .Concat((Config.AutoSpawnByLocation?.Cast<IAssetLocation>()).AsNonNullEnumerable())
            );
    }

}

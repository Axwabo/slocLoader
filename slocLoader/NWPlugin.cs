using AdminToys;
using HarmonyLib;
using MapGeneration;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using slocLoader.AutoObjectLoader;

namespace slocLoader;

public sealed class slocPlugin
{

    internal static slocPlugin Instance;

    private Harmony _harmony;

    [PluginConfig]
    public slocConfig Config = new();

    [PluginEntryPoint("slocLoader", "5.3.0", "A plugin that loads sloc files.", "Axwabo")]
    public void OnEnabled()
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

        if (Config.AutoLoad)
            try
            {
                AutomaticObjectLoader.LoadObjects();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load objects automatically:\n" + e);
            }

        API.UnsetPrefabs();
        EventManager.RegisterEvents(this);
        API.PrefabsLoaded += SpawnDefault;
        if (SeedSynchronizer.MapGenerated)
            API.LoadPrefabs();
        Logger.Info("slocLoader has been enabled");
    }

    [PluginUnload]
    public void OnDisabled()
    {
        _harmony.UnpatchAll();
        API.UnsetPrefabs();
        API.PrefabsLoaded -= SpawnDefault;
        EventManager.UnregisterEvents(this);
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
            Logger.Warning("Failed to find MapEditorReborn.Patches.UpdatePositionServerPatch type! Nested object scaling will behave weirdly!");
            return;
        }

        var instance = AccessTools.Property("MapEditorReborn.MapEditorReborn:Singleton")?.GetValue(null);
        if (instance == null)
        {
            Logger.Warning("Failed to find MapEditorReborn.MapEditorReborn.Singleton property! Nested object scaling will behave weirdly!");
            return;
        }

        if (AccessTools.Field("MapEditorReborn.MapEditorReborn:_harmony")?.GetValue(instance) is not Harmony harmony)
        {
            Logger.Warning("Failed to find MapEditorReborn.MapEditorReborn._harmony field! Nested object scaling will behave weirdly!");
            return;
        }

        harmony.Unpatch(AccessTools.Method(typeof(AdminToyBase), nameof(AdminToyBase.UpdatePositionServer)), patch);
        Logger.Info("Removed MapEditorReborn patch.");
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

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    private void MapGenerated() => API.LoadPrefabs();

}

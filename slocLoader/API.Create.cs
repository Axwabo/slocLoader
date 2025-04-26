using AdminToys;
using MapGeneration.Distributors;
using Mirror;
using slocLoader.Extensions;
using RelativePositioning;
using slocLoader.ObjectCreation;
using slocLoader.Objects;
using slocLoader.TriggerActions;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers;

namespace slocLoader;

public static partial class API
{

    public static slocGameObject CreateDefaultObject(this ObjectType type) => type switch
    {
        ObjectType.Cube
            or ObjectType.Sphere
            or ObjectType.Cylinder
            or ObjectType.Plane
            or ObjectType.Capsule
            or ObjectType.Quad => new PrimitiveObject(type),
        ObjectType.Light => new LightObject(),
        ObjectType.Empty => new EmptyObject(),
        ObjectType.Structure => new StructureObject(StructureObject.StructureType.None),
        _ => null
    };

    public static GameObject CreateObject(this slocGameObject obj, GameObject parent = null, bool throwOnError = true) => obj switch
    {
        StructureObject structure => CreateStructure(parent, structure),
        PrimitiveObject primitive => CreatePrimitive(parent, primitive),
        LightObject light => CreateLight(parent, light),
        EmptyObject => CreateEmpty(parent, obj),
        _ => throwOnError ? throw new IndexOutOfRangeException($"Unknown object type {obj.Type}") : null
    };

    public static readonly Dictionary<StructureObject.StructureType, uint> StructurePrefabIds = new()
    {
        {StructureObject.StructureType.Adrenaline, 2525847434},
        {StructureObject.StructureType.BinaryTarget, 3613149668},
        {StructureObject.StructureType.DboyTarget, 858699872},
        {StructureObject.StructureType.EzBreakableDoor, 1883254029},
        {StructureObject.StructureType.Generator, 2724603877},
        {StructureObject.StructureType.HczBreakableDoor, 2295511789},
        {StructureObject.StructureType.LargeGunLocker, 2830750618},
        {StructureObject.StructureType.LczBreakableDoor, 3038351124},
        {StructureObject.StructureType.Medkit, 4040822781},
        {StructureObject.StructureType.MiscellaneousLocker, 1964083310},
        {StructureObject.StructureType.RifleRack, 3352879624},
        {StructureObject.StructureType.Scp018Pedestal, 2286635216},
        {StructureObject.StructureType.Scp207Pedestal, 664776131},
        {StructureObject.StructureType.Scp244Pedestal, 3724306703},
        {StructureObject.StructureType.Scp268Pedestal, 3849573771},
        {StructureObject.StructureType.Scp500Pedestal, 373821065},
        {StructureObject.StructureType.Scp1576Pedestal, 3372339835},
        {StructureObject.StructureType.Scp1853Pedestal, 3962534659},
        {StructureObject.StructureType.Scp2176Pedestal, 3578915554},
        {StructureObject.StructureType.SportTarget, 1704345398},
        {StructureObject.StructureType.Workstation, 1783091262}
    };

    public static void ApplyAdminToyTransform(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out AdminToyBase toy))
            ApplyAdminToyTransform(toy);
    }

    public static void ApplyAdminToyTransform(AdminToyBase toy, bool hasCollider = true)
    {
        var t = toy.transform;
        toy.Position = t.localPosition;
        toy.Rotation = t.localRotation;
        toy.Scale = t.localScale;
    }

    private static GameObject CreateStructure(GameObject parent, StructureObject structure)
    {
        if (!StructurePrefabIds.TryGetValue(structure.Structure, out var id) || !NetworkClient.prefabs.TryGetValue(id, out var prefab))
            return null;
        var o = Object.Instantiate(prefab);
        o.SetAbsoluteTransformFrom(parent);
        o.SetLocalTransform(structure.Transform);
        o.ApplyNameAndTag(structure.Name, structure.Tag);
        o.AddComponent<slocObjectData>().GlobalTransform = o.TryGetComponent(out WaypointBase _);
        if (structure.RemoveDefaultLoot && o.TryGetComponent(out Locker locker))
            locker.Loot = [];
        ApplyAdminToyTransform(o);
        return o;
    }

    private static GameObject CreatePrimitive(GameObject parent, PrimitiveObject primitive)
    {
        if (PrimitivePrefab == null)
            throw new InvalidOperationException("Primitive prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(PrimitivePrefab);
        var flags = primitive.Flags;
        var primitiveType = primitive.Type.ToPrimitiveType();
        var o = toy.gameObject;
        o.ApplyNameAndTag(primitive.Name, primitive.Tag);
        var sloc = o.AddComponent<slocObjectData>();
        sloc.HasColliderOnClient = flags.HasFlagFast(PrimitiveObjectFlags.ClientCollider);
        sloc.ShouldBeSpawnedOnClient = !flags.HasFlagFast(PrimitiveObjectFlags.NotSpawned);
        sloc.IsTrigger = flags.IsTrigger();
        sloc.HasColliderOnServer = flags.HasFlagFast(PrimitiveObjectFlags.ServerCollider);
        toy.PrimitiveType = primitiveType;
        toy.MovementSmoothing = primitive.MovementSmoothing;
        toy.SetAbsoluteTransformFrom(parent);
        toy.SetLocalTransform(primitive.Transform);
        toy.MaterialColor = primitive.MaterialColor;
        AddActionHandlers(o, primitive.TriggerActions);
        ApplyAdminToyTransform(toy, sloc.HasColliderOnClient);
        if (!sloc.HasColliderOnClient)
            toy.PrimitiveFlags &= ~PrimitiveFlags.Collidable;
        toy.SetPrimitive(0, toy.PrimitiveType);
        return o;
    }

    private static GameObject CreateLight(GameObject parent, LightObject light)
    {
        if (LightPrefab == null)
            throw new InvalidOperationException("Light prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(LightPrefab);
        toy.SetAbsoluteTransformFrom(parent);
        toy.SetLocalTransform(light.Transform);
        toy.LightColor = light.LightColor;
        toy.LightIntensity = light.Intensity;
        toy.LightRange = light.Range;
        toy.LightType = light.LightType;
        toy.SpotAngle = light.SpotAngle;
        toy.InnerSpotAngle = light.InnerSpotAngle;
        toy.MovementSmoothing = light.MovementSmoothing;
        ApplyAdminToyTransform(toy);
        var go = toy.gameObject;
        go.ApplyNameAndTag(light.Name, light.Tag);
        go.AddComponent<slocObjectData>();
        return go;
    }

    private static GameObject CreateEmpty(GameObject parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        if (PrimitivePrefab == null)
            throw new InvalidOperationException("Primitive prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(PrimitivePrefab);
        toy.SetAbsoluteTransformFrom(parent);
        var t = toy.transform;
        t.localPosition = localPosition;
        t.localRotation = localRotation;
        t.localScale = localScale;
        toy.PrimitiveFlags = PrimitiveFlags.None;
        ApplyAdminToyTransform(toy);
        var data = toy.gameObject.AddComponent<slocObjectData>();
        data.HasColliderOnClient = false;
        data.HasColliderOnServer = false;
        return toy.gameObject;
    }

    private static GameObject CreateEmpty(GameObject parent, slocGameObject obj)
    {
        var go = CreateEmpty(parent, obj.Transform.Position, obj.Transform.Rotation, obj.Transform.Scale);
        go.ApplyNameAndTag(obj.Name, obj.Tag);
        return go;
    }

    private static void AddActionHandlers(GameObject o, BaseTriggerActionData[] data)
    {
        if (data is not {Length: not 0})
            return;
        var enter = new List<HandlerDataPair>();
        var stay = new List<HandlerDataPair>();
        var exit = new List<HandlerDataPair>();
        foreach (var action in data)
            if (action is SerializableTeleportToSpawnedObjectData tp)
                TpToSpawnedCache.GetOrAdd(o, () => []).Add(tp);
            else if (ActionManager.TryGetHandler(action.ActionType, out var handler))
                AddActionDataPairToList(action, handler, enter, stay, exit);
        if (enter.Count == 0 && stay.Count == 0 && exit.Count == 0)
            return;
        var component = o.GetOrAddComponent<TriggerListener>();
        component.OnEnter.AddRange(enter);
        component.OnStay.AddRange(stay);
        component.OnExit.AddRange(exit);
    }

    public static void AddActionDataPairToList(BaseTriggerActionData action, ITriggerActionHandler handler, List<HandlerDataPair> enter, List<HandlerDataPair> stay, List<HandlerDataPair> exit)
    {
        var e = action.SelectedEvents;
        if (e == TriggerEventType.None)
            return;
        var pair = new HandlerDataPair(action, handler);
        if (e.Is(TriggerEventType.Enter))
            enter.Add(pair);
        if (e.Is(TriggerEventType.Stay))
            stay.Add(pair);
        if (e.Is(TriggerEventType.Exit))
            exit.Add(pair);
    }

    private static readonly InstanceDictionary<GameObject> CreatedInstances = new();

    private static readonly Dictionary<GameObject, List<SerializableTeleportToSpawnedObjectData>> TpToSpawnedCache = new();

    #region Deprecated methods

    [Obsolete("Use CreateObjects(ObjectSource, Vector3, Quaternion) instead")]
    public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default)
        => CreateObjects(objects, out _, position, rotation);

    [Obsolete("Use CreateObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, out int createdAmount, Vector3 position, Quaternion rotation = default)
        => CreateObjects(ObjectsSource.From(objects), new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out createdAmount);

    [Obsolete("Use CreateObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject CreateObjectsFromStream(Stream objects, out int createdAmount, Vector3 position, Quaternion rotation = default)
        => CreateObjects(objects, new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out createdAmount);

    [Obsolete("Use CreateObjects(ObjectSource, out int, Vector3, Quaternion) instead")]
    public static GameObject CreateObjectsFromFile(string path, out int createdAmount, Vector3 position, Quaternion rotation = default)
        => CreateObjects(path, new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out createdAmount);

    #endregion

    public static GameObject CreateObjects(ObjectsSource source, CreateOptions options, out int createdAmount)
        => CreateOrSpawn(source, options, false, CreateObject, out createdAmount);

    public static GameObject CreateObjects(ObjectsSource source, CreateOptions options)
        => CreateObjects(source, options, out _);

    public static GameObject CreateObjects(ObjectsSource source, out int createdAmount, Vector3 position, Quaternion rotation = default)
        => CreateObjects(source, new CreateOptions
        {
            Position = position,
            Rotation = rotation
        }, out createdAmount);

    public static GameObject CreateObjects(ObjectsSource source, Vector3 position, Quaternion rotation = default)
        => CreateObjects(source, out _, position, rotation);

    private static void PostProcessSpecialTriggerActions()
    {
        if (!ActionManager.TryGetHandler(TriggerActionType.TeleportToSpawnedObject, out var handler))
            return;
        foreach (var kvp in TpToSpawnedCache)
        foreach (var data in kvp.Value)
            if (CreatedInstances.TryGetValue(data.ID, out var target))
                kvp.Key.AddTriggerAction(new RuntimeTeleportToSpawnedObjectData(target, data.Offset)
                {
                    SelectedTargets = data.SelectedTargets,
                    Options = data.Options,
                    RotationY = data.RotationY
                }, handler);
    }

}

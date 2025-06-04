using AdminToys;
using MapGeneration.Distributors;
using Mirror;
using RelativePositioning;
using slocLoader.Extensions;
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
        SpeakerObject speaker => CreateSpeaker(parent, speaker),
        CapybaraObject capybara => CreateCapybara(parent, capybara),
        StructureObject structure => CreateStructure(parent, structure),
        PrimitiveObject primitive => CreatePrimitive(parent, primitive),
        Scp079CameraObject camera => CreateCamera(parent, camera),
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

    public static readonly IReadOnlyDictionary<Scp079CameraType, uint> CameraTypeIds = new Dictionary<Scp079CameraType, uint>
    {
        {Scp079CameraType.LightContainmentZone, 2026969629},
        {Scp079CameraType.HeavyContainmentZone, 144958943},
        {Scp079CameraType.EntranceZone, 3375932423},
        {Scp079CameraType.EntranceZoneArm, 1824808402},
        {Scp079CameraType.SurfaceZone, 1734743361}
    };

    [Obsolete($"Use {nameof(AdminToyExtensions)}::{nameof(AdminToyExtensions.ApplyTransformNetworkProperties)} instead.")]
    public static void ApplyAdminToyTransform(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out AdminToyBase toy))
            ApplyAdminToyTransform(toy);
    }

    [Obsolete($"Use {nameof(AdminToyExtensions)}::{nameof(AdminToyExtensions.ApplyTransformNetworkProperties)} instead.")]
    public static void ApplyAdminToyTransform(AdminToyBase toy, bool hasCollider = true)
    {
        var t = toy.transform;
        toy.Position = t.localPosition;
        toy.Rotation = t.localRotation;
        toy.Scale = t.localScale;
    }

    private static GameObject CreateSpeaker(GameObject parent, SpeakerObject speaker)
    {
        if (SpeakerPrefab == null)
            throw new InvalidOperationException("Speaker prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(SpeakerPrefab);
        toy.ApplyCommonData(speaker, parent, out var go, out _);
        toy.ControllerId = speaker.ControllerId;
        toy.IsSpatial = speaker.Spatial;
        toy.Volume = speaker.Volume;
        toy.MinDistance = speaker.MinDistance;
        toy.MaxDistance = speaker.MaxDistance;
        return go;
    }

    private static GameObject CreateCapybara(GameObject parent, CapybaraObject capybara)
    {
        if (CapybaraPrefab == null)
            throw new InvalidOperationException("Capybara prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(CapybaraPrefab);
        toy.ApplyCommonData(capybara, parent, out var go, out _);
        toy.CollisionsEnabled = capybara.Collidable;
        return go;
    }

    private static GameObject CreateStructure(GameObject parent, StructureObject structure)
    {
        if (!StructurePrefabIds.TryGetValue(structure.Structure, out var id) || !NetworkClient.prefabs.TryGetValue(id, out var prefab))
            return null;
        var o = Object.Instantiate(prefab);
        o.ApplyCommonData(structure, parent, out var data);
        data.GlobalTransform = o.TryGetComponent(out WaypointBase _);
        if (structure.RemoveDefaultLoot && o.TryGetComponent(out Locker locker))
            locker.Loot = [];
        if (o.TryGetComponent(out StructurePositionSync sync))
            sync.Start();
        return o;
    }

    private static GameObject CreateCamera(GameObject parent, Scp079CameraObject camera)
    {
        if (!CameraTypeIds.TryGetValue(camera.CameraType, out var id)
            || !NetworkClient.prefabs.TryGetValue(id, out var prefab)
            || !prefab.TryGetComponent(out Scp079CameraToy prefabToy))
            return null;
        var toy = Object.Instantiate(prefabToy);
        toy.ApplyCommonData(camera, parent, out var o, out _);
        toy.NetworkLabel = camera.Label;
        toy.NetworkVerticalConstraint = new Vector2(camera.VerticalMinimum, camera.VerticalMaximum);
        toy.NetworkHorizontalConstraint = new Vector2(camera.HorizontalMinimum, camera.HorizontalMaximum);
        toy.NetworkZoomConstraint = new Vector2(camera.ZoomMinimum, camera.ZoomMaximum);
        toy.ApplyTransformNetworkProperties(camera.Transform.Position, camera.Transform.Rotation, camera.Transform.Scale);
        return o;
    }

    private static GameObject CreatePrimitive(GameObject parent, PrimitiveObject primitive)
    {
        if (PrimitivePrefab == null)
            throw new InvalidOperationException("Primitive prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(PrimitivePrefab);
        var flags = primitive.Flags;
        var primitiveType = primitive.Type.ToPrimitiveType();
        toy.ApplyCommonData(primitive, parent, out var o, out var sloc);
        toy.PrimitiveType = primitiveType;
        toy.MaterialColor = primitive.MaterialColor;
        sloc.HasColliderOnClient = flags.HasFlagFast(PrimitiveObjectFlags.ClientCollider);
        sloc.ShouldBeSpawnedOnClient = !flags.HasFlagFast(PrimitiveObjectFlags.NotSpawned);
        sloc.IsTrigger = flags.IsTrigger();
        sloc.HasColliderOnServer = flags.HasFlagFast(PrimitiveObjectFlags.ServerCollider);
        AddActionHandlers(o, primitive.TriggerActions);
        if (!flags.HasFlagFast(PrimitiveObjectFlags.Visible))
            toy.PrimitiveFlags &= ~PrimitiveFlags.Visible;
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
        toy.ApplyCommonData(light, parent, out var go, out _);
        toy.LightColor = light.LightColor;
        toy.LightIntensity = light.Intensity;
        toy.LightRange = light.Range;
        toy.LightType = light.LightType;
        toy.SpotAngle = light.SpotAngle;
        toy.InnerSpotAngle = light.InnerSpotAngle;
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
        toy.ApplyTransformNetworkProperties(localPosition, localRotation, localScale);
        toy.PrimitiveFlags = PrimitiveFlags.None;
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

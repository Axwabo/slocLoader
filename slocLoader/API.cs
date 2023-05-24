using System.Reflection;
using AdminToys;
using Mirror;
using slocLoader.Objects;
using slocLoader.Readers;
using slocLoader.TriggerActions;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers;

namespace slocLoader;

public static class API
{

    public const ushort slocVersion = 4;

    public const float ColorDivisionMultiplier = 1f / 255f;

    #region Prefabs

    public static PrimitiveObjectToy PrimitivePrefab { get; private set; }
    public static LightSourceToy LightPrefab { get; private set; }

    public static void LoadPrefabs()
    {
        foreach (var prefab in NetworkClient.prefabs.Values)
        {
            if (prefab.TryGetComponent(out PrimitiveObjectToy primitive))
                PrimitivePrefab = primitive;
            if (prefab.TryGetComponent(out LightSourceToy light))
                LightPrefab = light;
        }

        if (PrimitivePrefab == null || LightPrefab == null)
        {
            Log.Error("Either the primitive or light prefab is null. This should not happen!");
            return;
        }

        InvokeEvent();
    }

    private static void InvokeEvent()
    {
        if (PrefabsLoaded == null)
            return;
        foreach (var subscriber in PrefabsLoaded.GetInvocationList())
        {
            try
            {
                subscriber.DynamicInvoke();
            }
            catch (Exception e)
            {
                var method = subscriber.Method;
                var exception = e is TargetInvocationException {InnerException: { } inner} ? inner : e;
                Log.Error($"An exception was thrown by {method.DeclaringType?.FullName}::{method.Name} upon the invocation of PrefabsLoaded:\n{exception}");
            }
        }
    }

    internal static void UnsetPrefabs()
    {
        PrimitivePrefab = null;
        LightPrefab = null;
    }

    public static event Action PrefabsLoaded;

    #endregion

    #region Reader Declarations

    public static readonly IObjectReader DefaultReader = new Ver4Reader();

    private static readonly Dictionary<ushort, IObjectReader> VersionReaders = new()
    {
        {1, new Ver1Reader()},
        {2, new Ver2Reader()},
        {3, new Ver3Reader()},
        {4, new Ver4Reader()}
    };

    public static bool TryGetReader(ushort version, out IObjectReader reader) => VersionReaders.TryGetValue(version, out reader);

    public static IObjectReader GetReader(ushort version) => TryGetReader(version, out var reader) ? reader : DefaultReader;

    #endregion

    #region Read

    // starting from v3, the version is only a ushort instead of a uint
    private static ushort ReadVersionSafe(BufferedStream buffered, BinaryReader binaryReader)
    {
        var newVersion = binaryReader.ReadUInt16();
        var oldVersion = binaryReader.ReadUInt16();
        if (oldVersion is 0)
            return (ushort) (newVersion | (uint) oldVersion << 16);
        var newPos = buffered.Get<int>("_readPos") - sizeof(ushort);
        buffered.Set("_readPos", newPos); // rewind the buffer by two bytes, so the whole stream won't be malformed data
        return newVersion;
    }

    public static List<slocGameObject> ReadObjects(Stream stream, bool autoClose = true)
    {
        var objects = new List<slocGameObject>();
        using var buffered = new BufferedStream(stream, 4);
        var binaryReader = new BinaryReader(buffered);
        var version = ReadVersionSafe(buffered, binaryReader);
        if (!VersionReaders.ContainsKey(version))
            Log.Warn($"Unknown sloc version: {version}\nAttempting to read it using the default reader.");
        var reader = GetReader(version);
        var header = reader.ReadHeader(binaryReader);
        var objectCount = header.ObjectCount;
        for (var i = 0; i < objectCount; i++)
        {
            var obj = ReadObject(binaryReader, header, version, reader);
            if (obj is {IsValid: true})
                objects.Add(obj);
        }

        if (autoClose)
            binaryReader.Close();
        return objects;
    }

    public static List<slocGameObject> ReadObjectsFromFile(string path) => ReadObjects(File.OpenRead(path));

    #endregion

    #region Create

    public static slocGameObject CreateDefaultObject(this ObjectType type) => type switch
    {
        ObjectType.Cube
            or ObjectType.Sphere
            or ObjectType.Cylinder
            or ObjectType.Plane
            or ObjectType.Capsule
            or ObjectType.Quad => new PrimitiveObject(0, type),
        ObjectType.Light => new LightObject(0),
        ObjectType.Empty => new EmptyObject(0),
        _ => null
    };

    public static GameObject CreateObject(this slocGameObject obj, GameObject parent = null, bool throwOnError = true)
    {
        var transform = obj.Transform;
        return obj switch
        {
            PrimitiveObject primitive => CreatePrimitive(parent, primitive, transform),
            LightObject light => CreateLight(parent, transform, light),
            EmptyObject => CreateEmpty(parent, transform),
            _ => throwOnError ? throw new IndexOutOfRangeException($"Unknown object type {obj.Type}") : null
        };
    }

    private static GameObject CreatePrimitive(GameObject parent, PrimitiveObject primitive, slocTransform transform)
    {
        if (PrimitivePrefab == null)
            throw new InvalidOperationException("Primitive prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(PrimitivePrefab);
        var colliderMode = primitive.GetNonUnsetColliderMode();
        var primitiveType = primitive.Type.ToPrimitiveType();
        var o = toy.gameObject;
        var sloc = o.AddComponent<slocObjectData>();
        sloc.HasColliderOnClient = colliderMode is PrimitiveObject.ColliderCreationMode.ClientOnly or PrimitiveObject.ColliderCreationMode.Both;
        if (colliderMode is PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger or PrimitiveObject.ColliderCreationMode.ServerOnlyNonSpawned or PrimitiveObject.ColliderCreationMode.NoColliderNonSpawned)
            sloc.ShouldBeSpawnedOnClient = false;
        if (colliderMode is not (PrimitiveObject.ColliderCreationMode.NoCollider or PrimitiveObject.ColliderCreationMode.ClientOnly or PrimitiveObject.ColliderCreationMode.NoColliderNonSpawned))
            o.AddProperCollider(primitiveType, colliderMode.IsTrigger());
        AddActionHandlers(o, primitive);
        toy.PrimitiveType = primitiveType;
        toy.SetAbsoluteTransformFrom(parent);
        toy.SetLocalTransform(transform);
        toy.Scale = AdminToyPatch.GetScale(transform.Scale, sloc.HasColliderOnClient);
        toy.MaterialColor = primitive.MaterialColor;
        return o;
    }

    private static GameObject CreateLight(GameObject parent, slocTransform transform, LightObject light)
    {
        if (LightPrefab == null)
            throw new InvalidOperationException("Light prefab is not set! Make sure to spawn objects after the prefabs have been loaded.");
        var toy = Object.Instantiate(LightPrefab);
        toy.SetAbsoluteTransformFrom(parent);
        toy.SetLocalTransform(transform);
        toy.LightColor = light.LightColor;
        toy.LightShadows = light.Shadows;
        toy.LightRange = light.Range;
        toy.LightIntensity = light.Intensity;
        toy.Scale = transform.Scale;
        return toy.gameObject;
    }

    private static GameObject CreateEmpty(GameObject parent, slocTransform transform)
    {
        var emptyObject = new GameObject("Empty");
        emptyObject.SetAbsoluteTransformFrom(parent);
        emptyObject.SetLocalTransform(transform);
        return emptyObject;
    }

    private static void AddActionHandlers(GameObject o, PrimitiveObject primitive)
    {
        if (primitive.TriggerActions is not {Length: not 0})
            return;
        var enter = new List<HandlerDataPair>();
        var stay = new List<HandlerDataPair>();
        var exit = new List<HandlerDataPair>();
        foreach (var action in primitive.TriggerActions)
        {
            if (action is SerializableTeleportToSpawnedObjectData tp)
                TpToSpawnedCache.GetOrAdd(o, () => new List<SerializableTeleportToSpawnedObjectData>()).Add(tp);
            else if (ActionManager.TryGetHandler(action.ActionType, out var handler))
                AddActionDataPairToList(action, handler, enter, stay, exit);
        }

        if (enter.Count == 0 && stay.Count == 0 && exit.Count == 0)
            return;
        var component = o.AddComponent<TriggerListener>();
        component.OnEnter.AddRange(enter);
        component.OnStay.AddRange(stay);
        component.OnExit.AddRange(exit);
    }

    private static void AddActionDataPairToList(BaseTriggerActionData action, ITriggerActionHandler handler, List<HandlerDataPair> enter, List<HandlerDataPair> stay, List<HandlerDataPair> exit)
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

    public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) =>
        CreateObjects(objects, out _, position, rotation);

    public static GameObject CreateObjects(IEnumerable<slocGameObject> objects, out int createdAmount, Vector3 position, Quaternion rotation = default)
    {
        CreatedInstances.Clear();
        TpToSpawnedCache.Clear();
        try
        {
            var go = new GameObject
            {
                transform =
                {
                    position = position,
                    rotation = rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocObjectData>();
            createdAmount = 0;
            foreach (var o in objects)
            {
                var gameObject = o.CreateObject(CreatedInstances.GetOrReturn(o.ParentId, go, o.HasParent));
                if (gameObject == null)
                    continue;
                createdAmount++;
                CreatedInstances[o.InstanceId] = gameObject;
            }

            PostProcessSpecialTriggerActions();
            return go;
        }
        finally
        {
            CreatedInstances.Clear();
            TpToSpawnedCache.Clear();
        }
    }

    private static void PostProcessSpecialTriggerActions()
    {
        if (!ActionManager.TryGetHandler(TriggerActionType.TeleportToSpawnedObject, out var handler))
            return;
        foreach (var kvp in TpToSpawnedCache)
        foreach (var data in kvp.Value)
        {
            if (!CreatedInstances.TryGetValue(data.ID, out var target))
                continue;
            var component = kvp.Key.GetOrAddComponent<TriggerListener>();
            AddActionDataPairToList(
                new RuntimeTeleportToSpawnedObjectData(target, data.Offset)
                {
                    SelectedTargets = data.SelectedTargets,
                    Options = data.Options
                },
                handler,
                component.OnEnter,
                component.OnStay,
                component.OnExit
            );
        }
    }

    public static GameObject CreateObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default) =>
        CreateObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

    public static GameObject CreateObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default) =>
        CreateObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

    #endregion

    #region Spawn

    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, Vector3 position, Quaternion rotation = default) =>
        SpawnObjects(objects, out _, position, rotation);

    public static GameObject SpawnObjects(IEnumerable<slocGameObject> objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
    {
        CreatedInstances.Clear();
        TpToSpawnedCache.Clear();
        try
        {
            var go = new GameObject
            {
                transform =
                {
                    position = position,
                    rotation = rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocObjectData>();
            NetworkServer.Spawn(go);
            spawnedAmount = 0;
            foreach (var o in objects)
            {
                var gameObject = o.SpawnObject(CreatedInstances.GetOrReturn(o.ParentId, go, o.HasParent));
                if (gameObject == null)
                    continue;
                spawnedAmount++;
                CreatedInstances[o.InstanceId] = gameObject;
            }

            PostProcessSpecialTriggerActions();
            return go;
        }
        finally
        {
            CreatedInstances.Clear();
            TpToSpawnedCache.Clear();
        }
    }

    public static GameObject SpawnObjectsFromStream(Stream objects, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjects(objects), out spawnedAmount, position, rotation);

    public static GameObject SpawnObjectsFromFile(string path, out int spawnedAmount, Vector3 position, Quaternion rotation = default)
        => SpawnObjects(ReadObjectsFromFile(path), out spawnedAmount, position, rotation);

    public static GameObject SpawnObject(this slocGameObject obj, GameObject parent = null, bool throwOnError = true)
    {
        var o = CreateObject(obj, parent, throwOnError);
        if (o == null)
        {
            if (throwOnError)
                throw new ArgumentOutOfRangeException(nameof(obj.Type), obj.Type, "Unknown object type");
            return null;
        }

        if (!o.TryGetComponent(out slocObjectData data) || data.ShouldBeSpawnedOnClient)
            NetworkServer.Spawn(o);
        return o;
    }

    #endregion

    #region BinaryReader Extensions

    public static slocGameObject ReadObject(this BinaryReader stream, slocHeader header, ushort version = 0, IObjectReader objectReader = null)
    {
        objectReader ??= GetReader(version);
        return objectReader.Read(stream, header);
    }

    public static slocTransform ReadTransform(this BinaryReader reader) => new()
    {
        Position = reader.ReadVector(),
        Scale = reader.ReadVector(),
        Rotation = reader.ReadQuaternion()
    };

    public static Vector3 ReadVector(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Quaternion ReadQuaternion(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Color ReadColor(this BinaryReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Color ReadLossyColor(this BinaryReader reader)
    {
        var color = reader.ReadInt32();
        var red = color >> 24 & 0xFF;
        var green = color >> 16 & 0xFF;
        var blue = color >> 8 & 0xFF;
        var alpha = color & 0xFF;
        return new(red * ColorDivisionMultiplier, green * ColorDivisionMultiplier, blue * ColorDivisionMultiplier, alpha * ColorDivisionMultiplier);
    }

    public static int ReadObjectCount(this BinaryReader reader)
    {
        var count = reader.ReadInt32();
        return count < 0 ? 0 : count;
    }

    public static float ReadShortAsFloat(this BinaryReader reader) => reader.ReadInt16() * TeleporterImmunityData.ShortToFloatMultiplier;

    #endregion

    #region BinaryWriter Extensions

    public static void WriteVector(this BinaryWriter writer, Vector3 vector3)
    {
        writer.Write(vector3.x);
        writer.Write(vector3.y);
        writer.Write(vector3.z);
    }

    public static void WriteQuaternion(this BinaryWriter writer, Quaternion quaternion)
    {
        writer.Write(quaternion.x);
        writer.Write(quaternion.y);
        writer.Write(quaternion.z);
        writer.Write(quaternion.w);
    }

    public static void WriteColor(this BinaryWriter writer, Color color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    public static void WriteFloatAsShort(this BinaryWriter writer, float value) => writer.Write((ushort) Mathf.Floor(value * TeleporterImmunityData.FloatToShortMultiplier));

    #endregion

    #region Bit Math

    public static PrimitiveObject.ColliderCreationMode CombineSafe(PrimitiveObject.ColliderCreationMode a, PrimitiveObject.ColliderCreationMode b) =>
        (PrimitiveObject.ColliderCreationMode) CombineSafe((byte) a, (byte) b);

    public static void SplitSafe(PrimitiveObject.ColliderCreationMode combined, out PrimitiveObject.ColliderCreationMode a, out PrimitiveObject.ColliderCreationMode b)
    {
        SplitSafe((byte) combined, out var x, out var y);
        a = (PrimitiveObject.ColliderCreationMode) x;
        b = (PrimitiveObject.ColliderCreationMode) y;
    }

    public static byte CombineSafe(byte a, byte b) => (byte) (a << 4 | b);

    public static void SplitSafe(byte combined, out byte a, out byte b)
    {
        a = (byte) (combined >> 4);
        b = (byte) (combined & 0xF);
    }

    #endregion

    public static PrimitiveType ToPrimitiveType(this ObjectType type) => type switch
    {
        ObjectType.Cube => PrimitiveType.Cube,
        ObjectType.Sphere => PrimitiveType.Sphere,
        ObjectType.Capsule => PrimitiveType.Capsule,
        ObjectType.Cylinder => PrimitiveType.Cylinder,
        ObjectType.Plane => PrimitiveType.Plane,
        ObjectType.Quad => PrimitiveType.Quad,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A non-primitive type was supplied")
    };

    public static void SetAbsoluteTransformFrom(this Component component, GameObject parent)
    {
        if (component != null)
            SetAbsoluteTransformFrom(component.gameObject, parent);
    }

    public static void SetLocalTransform(this Component component, slocTransform transform)
    {
        if (component != null)
            SetLocalTransform(component.gameObject, transform);
    }

    public static void SetAbsoluteTransformFrom(this GameObject o, GameObject parent)
    {
        if (parent != null)
            o.transform.SetParent(parent.transform, false);
    }

    public static void SetLocalTransform(this GameObject o, slocTransform transform)
    {
        if (o == null)
            return;
        var t = o.transform;
        t.localScale = transform.Scale;
        t.localPosition = transform.Position;
        t.localRotation = transform.Rotation;
    }

    public static IEnumerable<GameObject> WithAllChildren(this GameObject o) => o.GetComponentsInChildren<Transform>().Select(e => e.gameObject);

    public static bool HasFlagFast(this slocAttributes attributes, slocAttributes flag) => (attributes & flag) == flag;

    public static int ToRgbRange(this float f) => Mathf.FloorToInt(Mathf.Clamp01(f) * 255f);

    public static int ToLossyColor(this Color color) => color.r.ToRgbRange() << 24 | color.g.ToRgbRange() << 16 | color.b.ToRgbRange() << 8 | color.a.ToRgbRange();

    public static Collider AddProperCollider(this GameObject o, PrimitiveType type, bool isTrigger)
    {
        Collider collider = type switch
        {
            PrimitiveType.Cube => o.AddComponent<BoxCollider>(),
            PrimitiveType.Sphere => o.AddComponent<SphereCollider>(),
            PrimitiveType.Capsule or PrimitiveType.Cylinder => o.AddComponent<CapsuleCollider>(),
            PrimitiveType.Plane => o.AddComponent<BoxCollider>(),
            PrimitiveType.Quad => o.AddComponent<BoxCollider>(),
            _ => null
        };
        if (collider && isTrigger)
            collider.isTrigger = true;
        return collider;
    }

    public static bool IsTrigger(this PrimitiveObject.ColliderCreationMode colliderMode) => colliderMode is PrimitiveObject.ColliderCreationMode.Trigger or PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger;

    public static bool HasAttribute(this slocHeader header, slocAttributes attribute) => (header.Attributes & attribute) == attribute;

}

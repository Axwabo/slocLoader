using System.Reflection;
using AdminToys;
using Mirror;
using slocLoader.ObjectCreation;
using slocLoader.Objects;
using slocLoader.Readers;
using slocLoader.TriggerActions;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers;

namespace slocLoader;

public static partial class API
{

    public const ushort slocVersion = 5;

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
        var collider = o.GetOrAddComponent<MeshCollider>();
        collider.convex = type is not PrimitiveType.Plane and not PrimitiveType.Quad;
        collider.isTrigger = isTrigger;
        return collider;
    }

    public static bool IsTrigger(this PrimitiveObject.ColliderCreationMode colliderMode) => colliderMode is PrimitiveObject.ColliderCreationMode.Trigger or PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger;

    public static bool HasAttribute(this slocHeader header, slocAttributes attribute) => (header.Attributes & attribute) == attribute;

    public static void AddTriggerAction(this PrimitiveObjectToy toy, BaseTriggerActionData data, ITriggerActionHandler handler = null)
    {
        if (handler == null && !ActionManager.TryGetHandler(data.ActionType, out handler))
            throw new ArgumentException("The required action handler could not be found based on the provided data. To use a custom handler, specify the handler yourself.", nameof(data));
        var listener = toy.GetOrAddComponent<TriggerListener>();
        AddActionDataPairToList(data, handler, listener.OnEnter, listener.OnStay, listener.OnExit);
    }

    public static void AddTriggerAction(this GameObject gameObject, BaseTriggerActionData data, ITriggerActionHandler handler)
    {
        if (!gameObject.TryGetComponent(out PrimitiveObjectToy toy))
            throw new ArgumentException("The provided game object does not have a PrimitiveObjectToy component.", nameof(gameObject));
        toy.AddTriggerAction(data, handler);
    }

    private static GameObject CreateOrSpawn(ObjectsSource source, CreateOptions options, bool spawnRoot, Func<slocGameObject, GameObject, bool, GameObject> createMethod, out int createdAmount)
    {
        if (source.Objects == null)
            throw new ArgumentException("Source is null.", nameof(source));
        CreatedInstances.Clear();
        TpToSpawnedCache.Clear();
        try
        {
            var go = new GameObject
            {
                transform =
                {
                    position = options.Position,
                    rotation = options.Rotation,
                }
            };
            go.AddComponent<NetworkIdentity>();
            go.AddComponent<slocObjectData>();
            if (spawnRoot)
                NetworkServer.Spawn(go);
            createdAmount = 0;
            foreach (var o in source)
            {
                var previousSmoothing = o.MovementSmoothing;
                if (options.MovementSmoothing != null)
                    o.MovementSmoothing = options.MovementSmoothing.Value;
                var gameObject = createMethod(o, CreatedInstances.GetOrReturn(o.ParentId, go, o.HasParent), true);
                o.MovementSmoothing = previousSmoothing;
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

}

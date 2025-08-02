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

    public const ushort slocVersion = 6;

    public const float ColorDivisionMultiplier = 1f / 255f;

    #region Prefabs

    public static PrimitiveObjectToy PrimitivePrefab { get; private set; }
    public static LightSourceToy LightPrefab { get; private set; }
    public static CapybaraToy CapybaraPrefab { get; private set; }
    public static SpeakerToy SpeakerPrefab { get; private set; }
    public static InvisibleInteractableToy InteractablePrefab { get; private set; }
    public static TextToy TextPrefab { get; private set; }
    public static WaypointToy WaypointPrefab { get; private set; }
    public static SpawnableCullingParent CullingParentPrefab { get; private set; }

    public static void LoadPrefabs()
    {
        foreach (var prefab in NetworkClient.prefabs.Values)
            if (prefab.TryGetComponent(out PrimitiveObjectToy primitive))
                PrimitivePrefab = primitive;
            else if (prefab.TryGetComponent(out LightSourceToy light))
                LightPrefab = light;
            else if (prefab.TryGetComponent(out CapybaraToy capybara))
                CapybaraPrefab = capybara;
            else if (prefab.TryGetComponent(out SpeakerToy speaker))
                SpeakerPrefab = speaker;
            else if (prefab.TryGetComponent(out InvisibleInteractableToy interactable))
                InteractablePrefab = interactable;
            else if (prefab.TryGetComponent(out TextToy text))
                TextPrefab = text;
            else if (prefab.TryGetComponent(out WaypointToy waypoint))
                WaypointPrefab = waypoint;
            else if (prefab.TryGetComponent(out SpawnableCullingParent cullingParent))
                CullingParentPrefab = cullingParent;
        OnPrefabsProcessed();
    }

    private static void OnPrefabsProcessed()
    {
        if (PrimitivePrefab && LightPrefab && CapybaraPrefab && SpeakerPrefab && InteractablePrefab && TextPrefab && WaypointPrefab && CullingParentPrefab)
            InvokeEvent();
        else
            Logger.Error("Either the primitive, light, capybara, speaker, interactable, text, waypoint or culling parent prefab is null. This should not happen!");
    }

    private static void InvokeEvent()
    {
        if (PrefabsLoaded == null)
            return;
        foreach (var subscriber in PrefabsLoaded.GetInvocationList())
            try
            {
                subscriber.DynamicInvoke();
            }
            catch (Exception e)
            {
                var method = subscriber.Method;
                var exception = e is TargetInvocationException {InnerException: { } inner} ? inner : e;
                Logger.Error($"An exception was thrown by {method.DeclaringType?.FullName}::{method.Name} upon the invocation of PrefabsLoaded:\n{exception}");
            }
    }

    internal static void UnsetPrefabs()
    {
        PrimitivePrefab = null;
        LightPrefab = null;
        CapybaraPrefab = null;
        SpeakerPrefab = null;
        InteractablePrefab = null;
        TextPrefab = null;
        WaypointPrefab = null;
        CullingParentPrefab = null;
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
        t.SetLocalPositionAndRotation(transform.Position, transform.Rotation);
        t.localScale = transform.Scale;
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

    [Obsolete("Collider modes have been replaced by primitive flags.")]
    public static bool IsTrigger(this PrimitiveObject.ColliderCreationMode colliderMode) => colliderMode is PrimitiveObject.ColliderCreationMode.Trigger or PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger;

    public static bool HasAttribute(this slocHeader header, slocAttributes attribute) => (header.Attributes & attribute) == attribute;

    public static void AddTriggerAction(this PrimitiveObjectToy toy, BaseTriggerActionData data, ITriggerActionHandler handler = null)
    {
        if (handler == null && !ActionManager.TryGetHandler(data.ActionType, out handler))
            throw new ArgumentException("The required action handler could not be found based on the provided data. To use a custom handler, specify the handler yourself.", nameof(data));
        var listener = toy.GetOrAddComponent<TriggerListener>();
        AddActionDataPairToList(data, handler, listener.OnEnter, listener.OnStay, listener.OnExit);
    }

    public static void AddTriggerAction(this GameObject gameObject, BaseTriggerActionData data, ITriggerActionHandler handler = null)
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
            var go = CreateEmpty(options.Parent, options.Position, options.Rotation, options.Scale);
            var root = go.GetComponent<AdminToyBase>();
            root.IsStatic = options.StaticRoot;
            root.MovementSmoothing = options.RootSmoothing;
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
                if (gameObject.TryGetComponent(out AdminToyBase toy) && toy is not WaypointToy)
                    toy.IsStatic = options.IsStatic;
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

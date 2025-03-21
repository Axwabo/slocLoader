using slocLoader.Objects;

#pragma warning disable CS0618 // Type or member is obsolete

namespace slocLoader.Extensions;

public static class ColliderModeCompatibility
{

    public static PrimitiveObjectFlags GetPrimitiveFlags(PrimitiveObject.ColliderCreationMode mode) => PrimitiveObjectFlags.Visible | mode switch
    {
        PrimitiveObject.ColliderCreationMode.Unset => PrimitiveObjectFlags.None,
        PrimitiveObject.ColliderCreationMode.NoCollider => PrimitiveObjectFlags.None,
        PrimitiveObject.ColliderCreationMode.ClientOnly => PrimitiveObjectFlags.ClientCollider,
        PrimitiveObject.ColliderCreationMode.ServerOnly => PrimitiveObjectFlags.ServerCollider,
        PrimitiveObject.ColliderCreationMode.Both => PrimitiveObjectFlags.ClientCollider | PrimitiveObjectFlags.ServerCollider,
        PrimitiveObject.ColliderCreationMode.Trigger => PrimitiveObjectFlags.ServerCollider | PrimitiveObjectFlags.Trigger,
        PrimitiveObject.ColliderCreationMode.NonSpawnedTrigger => PrimitiveObjectFlags.ServerCollider | PrimitiveObjectFlags.Trigger | PrimitiveObjectFlags.NotSpawned,
        PrimitiveObject.ColliderCreationMode.ServerOnlyNonSpawned => PrimitiveObjectFlags.ServerCollider | PrimitiveObjectFlags.NotSpawned,
        PrimitiveObject.ColliderCreationMode.NoColliderNonSpawned => PrimitiveObjectFlags.NotSpawned,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown collider creation mode")
    };

}

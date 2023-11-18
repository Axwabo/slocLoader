using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToSpawnedObjectHandler : TeleportHandlerBase<RuntimeTeleportToSpawnedObjectData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

    protected override bool ValidateData(GameObject interactingObject, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener)
        => data.Target != null && !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override bool TryCalculateTransform(RuntimeTeleportToSpawnedObjectData data, out Vector3 position, out Quaternion rotation)
    {
        data.ToWorldSpace(data.Target.transform, out position, out rotation);
        return true;
    }

}

using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToSpawnedObjectHandler : TeleportHandlerBase<RuntimeTeleportToSpawnedObjectData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

    protected override bool ValidateData(GameObject interactingObject, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener)
        => data.Target != null && !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override Transform GetReferenceTransform(Component component, RuntimeTeleportToSpawnedObjectData data) => data.Target.transform;

}

using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToSpawnedObjectHandler : UniversalTriggerActionHandler<RuntimeTeleportToSpawnedObjectData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

    protected override bool ValidateData(GameObject interactingObject, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener) =>
        data.Target != null && !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener) =>
        player.OverridePosition(data.ToWorldSpacePosition(data.Target.transform), data.Options);

    protected override void HandleItem(ItemPickupBase pickup, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, RuntimeTeleportToSpawnedObjectData data, TriggerListener listener) =>
        TriggerActionHelpers.SetRagdollPosition(ragdoll, data.ToWorldSpacePosition(data.Target.transform));

    private static void HandleComponent(Component component, RuntimeTeleportToSpawnedObjectData data) =>
        component.transform.position = data.ToWorldSpacePosition(data.Target.transform);

}

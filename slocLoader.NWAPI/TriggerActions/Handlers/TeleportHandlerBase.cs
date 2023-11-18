using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public abstract class TeleportHandlerBase<TData> : UniversalTriggerActionHandler<TData> where TData : BaseTeleportData
{

    protected override bool ValidateData(GameObject interactingObject, TData data, TriggerListener listener)
        => !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, TData data, TriggerListener listener)
    {
        if (TryCalculateTransform(data, out var pos, out var rotation))
            player.OverridePosition(pos, data.Options, rotation.y);
    }

    protected override void HandleItem(ItemPickupBase pickup, TData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, TData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, TData data, TriggerListener listener)
    {
        if (TryCalculateTransform(data, out var pos, out var rotation))
            TriggerActionHelpers.SetRagdollPositionAndRotation(ragdoll, pos, rotation);
    }

    protected abstract bool TryCalculateTransform(TData data, out Vector3 vector, out Quaternion rotation);

    protected void HandleComponent(Component component, TData data)
    {
        if (!TryCalculateTransform(data, out var pos, out var rotation))
            return;
        var t = component.transform;
        t.position = pos;
        t.rotation = rotation;
    }

}

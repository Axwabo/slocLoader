using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Handlers.Abstract;

public abstract class TeleportHandlerBase<TData> : UniversalTriggerActionHandler<TData> where TData : BaseTeleportData
{

    protected override bool ValidateData(GameObject interactingObject, TData data, TriggerListener listener)
        => !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, TData data, TriggerListener listener)
    {
        if (TryCalculateTransform(player, data, out var pos, out var rotation))
            player.OverridePosition(pos + Vector3.up, data.Options & ~TeleportOptions.DeltaRotation, rotation.eulerAngles.y);
    }

    protected override void HandleItem(ItemPickupBase pickup, TData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, TData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, TData data, TriggerListener listener)
    {
        if (TryCalculateTransform(ragdoll, data, out var pos, out var rotation))
            TriggerActionHelpers.SetRagdollPositionAndRotation(ragdoll, pos, rotation);
    }

    protected abstract Transform GetReferenceTransform(Component component, TData data);

    protected bool TryCalculateTransform(Component component, TData data, out Vector3 position, out Quaternion rotation)
    {
        Transform reference = GetReferenceTransform(component, data);
        float y;
        if (reference)
            data.ToWorldSpace(reference, out position, out y);
        else
        {
            position = data.Position;
            y = data.RotationY;
        }

        var euler = component.transform.rotation.eulerAngles;
        rotation = Quaternion.Euler(euler.x, data.Options.HasFlagFast(TeleportOptions.DeltaRotation) ? euler.y + data.RotationY : y, euler.z);
        return true;
    }

    protected void HandleComponent(Component component, TData data)
    {
        if (!TryCalculateTransform(component, data, out var pos, out var rotation))
            return;
        var t = component.transform;
        t.position = pos;
        t.rotation = rotation;
    }

}

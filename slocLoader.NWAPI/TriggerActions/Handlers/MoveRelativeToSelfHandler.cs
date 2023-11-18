using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class MoveRelativeToSelfHandler : UniversalTriggerActionHandler<MoveRelativeToSelfData>
{

    public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

    protected override bool ValidateData(GameObject interactingObject, MoveRelativeToSelfData data, TriggerListener listener)
        => !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, MoveRelativeToSelfData data, TriggerListener listener)
        => player.OverridePosition(data.Position, data.Options);

    protected override void HandleItem(ItemPickupBase pickup, MoveRelativeToSelfData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, MoveRelativeToSelfData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, MoveRelativeToSelfData data, TriggerListener listener)
    {
        data.ToWorldSpace(ragdoll.transform, out var position, out var rotation);
        TriggerActionHelpers.SetRagdollPositionAndRotation(ragdoll, position, rotation);
    }

    private static void HandleComponent(Component component, MoveRelativeToSelfData data)
    {
        var t = component.transform;
        data.ToWorldSpace(t, out var position, out var rotation);
        t.position = position;
        t.rotation = rotation;
    }

}

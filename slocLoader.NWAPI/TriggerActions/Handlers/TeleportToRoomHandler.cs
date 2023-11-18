using AdminToys;
using Axwabo.Helpers.Config;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToRoomHandler : UniversalTriggerActionHandler<TeleportToRoomData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

    protected override bool ValidateData(GameObject interactingObject, TeleportToRoomData data, TriggerListener listener) =>
        !string.IsNullOrWhiteSpace(data.Room) && !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, TeleportToRoomData data, TriggerListener listener)
    {
        if (TryCalculatePosition(data, out var pos))
            player.OverridePosition(pos, data.Options);
    }

    protected override void HandleItem(ItemPickupBase pickup, TeleportToRoomData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, TeleportToRoomData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, TeleportToRoomData data, TriggerListener listener)
    {
        if (TryCalculatePosition(data, out var pos))
            TriggerActionHelpers.SetRagdollPosition(ragdoll, pos);
    }

    private static bool TryCalculatePosition(TeleportToRoomData data, out Vector3 vector)
    {
        var point = new MapPointByName(data.Room, data.Position);
        if (!data.Options.HasFlagFast(TeleportOptions.WorldSpacePosition))
            return point.TryGetWorldPose(out vector, out _);
        var transform = point.RoomTransform();
        if (!transform)
        {
            vector = Vector3.zero;
            return false;
        }

        vector = transform.position + data.Position;
        return true;
    }

    private static void HandleComponent(Component component, TeleportToRoomData data)
    {
        if (TryCalculatePosition(data, out var pos))
            component.transform.position = pos;
    }

}

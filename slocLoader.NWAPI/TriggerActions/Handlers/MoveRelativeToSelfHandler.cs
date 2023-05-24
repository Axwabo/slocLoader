using AdminToys;
using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class MoveRelativeToSelfHandler : UniversalTriggerActionHandler<MoveRelativeToSelfData>
{

    public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

    protected override bool ValidateData(GameObject interactingObject, MoveRelativeToSelfData data, TriggerListener listener) =>
        !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, MoveRelativeToSelfData data, TriggerListener listener)
    {
        if (!player.TryGetMovementModule(out var module))
            return;
        var offset = !data.Options.HasFlagFast(TeleportOptions.WorldSpaceTransform) ? player.PlayerCameraReference.rotation * data.Position : data.Position;
        if (data.Options.HasFlagFast(TeleportOptions.ResetFallDamage))
            module.Motor.ResetFallDamageCooldown();
        module.ServerOverridePosition(module.Position + offset, Vector3.zero);
    }

    protected override void HandleItem(ItemPickupBase pickup, MoveRelativeToSelfData data, TriggerListener listener)
    {
        TriggerActionHelpers.ResetVelocityOfPickup(pickup, data.Options);
        HandleComponent(pickup, data);
    }

    protected override void HandleToy(AdminToyBase toy, MoveRelativeToSelfData data, TriggerListener listener) => HandleComponent(toy, data);

    protected override void HandleRagdoll(BasicRagdoll ragdoll, MoveRelativeToSelfData data, TriggerListener listener) =>
        TriggerActionHelpers.SetRagdollPosition(ragdoll, data.ToWorldSpacePosition(ragdoll.transform));

    private static void HandleComponent(Component component, MoveRelativeToSelfData data)
    {
        var t = component.transform;
        t.position = data.ToWorldSpacePosition(t);
    }

}

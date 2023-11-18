using AdminToys;
using InventorySystem.Items.Pickups;
using PlayerRoles.Ragdolls;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class MoveRelativeToSelfHandler : TeleportHandlerBase<MoveRelativeToSelfData>
{

    public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

    private Component _currentComponent;

    protected override bool ValidateData(GameObject interactingObject, MoveRelativeToSelfData data, TriggerListener listener)
        => !TeleporterImmunityStorage.IsImmune(interactingObject, listener);

    protected override void HandlePlayer(ReferenceHub player, MoveRelativeToSelfData data, TriggerListener listener)
    {
        _currentComponent = player;
        base.HandlePlayer(player, data, listener);
    }

    protected override void HandleItem(ItemPickupBase pickup, MoveRelativeToSelfData data, TriggerListener listener)
    {
        _currentComponent = pickup;
        base.HandleItem(pickup, data, listener);
    }

    protected override void HandleToy(AdminToyBase toy, MoveRelativeToSelfData data, TriggerListener listener)
    {
        _currentComponent = toy;
        base.HandleToy(toy, data, listener);
    }

    protected override void HandleRagdoll(BasicRagdoll ragdoll, MoveRelativeToSelfData data, TriggerListener listener)
    {
        _currentComponent = ragdoll;
        base.HandleRagdoll(ragdoll, data, listener);
    }

    protected override bool TryCalculateTransform(MoveRelativeToSelfData data, out Vector3 position, out Quaternion rotation)
    {
        data.ToWorldSpace(_currentComponent.transform, out position, out rotation);
        return true;
    }

}

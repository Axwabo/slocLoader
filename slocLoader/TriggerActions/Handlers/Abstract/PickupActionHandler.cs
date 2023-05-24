using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;

namespace slocLoader.TriggerActions.Handlers.Abstract;

public abstract class PickupActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData
{

    public TargetType Targets => TargetType.Pickup;

    public abstract TriggerActionType ActionType { get; }

    public void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener)
    {
        if (data is TData t && interactingObject.TryGetComponent(out ItemPickupBase pickup))
            HandlePickup(pickup, t);
    }

    protected abstract void HandlePickup(ItemPickupBase player, TData data);

}

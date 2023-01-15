using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class PickupActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.Pickup;
        
        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is TData t && obj.TryGetComponent(out ItemPickupBase pickup))
                HandlePlayer(pickup, t);
        }

        protected abstract void HandlePlayer(ItemPickupBase player, TData data);

    }

}

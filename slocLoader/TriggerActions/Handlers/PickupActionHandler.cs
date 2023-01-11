using InventorySystem.Items.Pickups;
using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public abstract class PickupActionHandler<T> : ITriggerActionHandler where T : BaseTriggerActionData {

        public TargetType Targets => TargetType.Pickup;
        
        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is T t && obj.TryGetComponent(out ItemPickupBase pickup))
                HandlePlayer(pickup, t);
        }

        protected abstract void HandlePlayer(ItemPickupBase player, T data);

    }

}

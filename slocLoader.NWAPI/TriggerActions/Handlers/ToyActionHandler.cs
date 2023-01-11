using AdminToys;
using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public abstract class ToyActionHandler<T> : ITriggerActionHandler where T : BaseTriggerActionData {

        public TargetType Targets => TargetType.Toy;
        
        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject obj, BaseTriggerActionData data) {
            if (data is T t && obj.TryGetComponent(out AdminToyBase toy))
                HandlePlayer(toy, t);
        }

        protected abstract void HandlePlayer(AdminToyBase player, T data);

    }

}

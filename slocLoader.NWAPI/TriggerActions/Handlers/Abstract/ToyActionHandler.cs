using AdminToys;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers.Abstract {

    public abstract class ToyActionHandler<TData> : ITriggerActionHandler where TData : BaseTriggerActionData {

        public TargetType Targets => TargetType.Toy;

        public abstract TriggerActionType ActionType { get; }

        public void HandleObject(GameObject interactingObject, BaseTriggerActionData data, TriggerListener listener) {
            if (data is TData t && interactingObject.TryGetComponent(out AdminToyBase toy))
                HandleToy(toy, t);
        }

        protected abstract void HandleToy(AdminToyBase player, TData data);

    }

}

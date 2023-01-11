using slocLoader.TriggerActions.Data;
using UnityEngine;

namespace slocLoader.TriggerActions.Handlers {

    public interface ITriggerActionHandler {

        TargetType Targets { get; }
        
        TriggerActionType ActionType { get; }

        void HandleObject(GameObject obj, BaseTriggerActionData data);

    }

}

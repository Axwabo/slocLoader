using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Handlers;

namespace slocLoader.TriggerActions {

    public readonly struct HandlerDataPair {

        public readonly BaseTriggerActionData Data;

        public readonly ITriggerActionHandler Handler;

        public HandlerDataPair(BaseTriggerActionData data, ITriggerActionHandler handler) {
            Data = data;
            Handler = handler;
        }

    }

}

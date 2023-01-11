using UnityEngine;

namespace slocLoader.TriggerActions {

    [DisallowMultipleComponent]
    public sealed class TriggerListener : MonoBehaviour {

        public HandlerDataPair[] ActionHandlers;

        private void OnTriggerEnter(Collider other) {
            if (ActionHandlers == null)
                return;
            var go = other.gameObject;
            foreach (var pair in ActionHandlers)
                pair.Handler.HandleObject(go, pair.Data);
        }

    }

}

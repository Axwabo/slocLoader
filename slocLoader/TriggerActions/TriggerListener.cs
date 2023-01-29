using System.Collections.Generic;
using UnityEngine;

namespace slocLoader.TriggerActions {

    [DisallowMultipleComponent]
    public sealed class TriggerListener : MonoBehaviour {

        public readonly List<HandlerDataPair> ActionHandlers = new();

        private void OnTriggerEnter(Collider other) {
            var go = other.gameObject.transform.root.gameObject;
            foreach (var pair in ActionHandlers)
                pair.Handler.HandleObject(go, pair.Data);
        }

    }

}

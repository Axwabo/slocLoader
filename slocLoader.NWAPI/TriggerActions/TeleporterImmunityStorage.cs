using System.Collections.Generic;
using UnityEngine;

namespace slocLoader.TriggerActions {

    [DisallowMultipleComponent]
    public sealed class TeleporterImmunityStorage : MonoBehaviour {

        private bool _isTriggerProcessActive;

        public bool IsTriggerProcessActive {
            get => _isTriggerProcessActive;
            set {
                RefreshImmunityFrame();
                _isTriggerProcessActive = value;
            }
        }

        private void RefreshImmunityFrame() {
            _wasGloballyImmune = Time.timeSinceLevelLoad < GloballyImmuneUntil;
            foreach (var kvp in new Dictionary<int, KeyValuePair<float, bool>>(_locallyImmuneUntil)) {
                var data = kvp.Value;
                _locallyImmuneUntil[kvp.Key] = new KeyValuePair<float, bool>(data.Key, Time.timeSinceLevelLoad < data.Key);
            }
        }

        private bool _wasGloballyImmune;

        public float GloballyImmuneUntil { get; set; }

        private readonly InstanceDictionary<KeyValuePair<float, bool>> _locallyImmuneUntil = new();

        public bool IsGloballyImmune => IsTriggerProcessActive ? _wasGloballyImmune : Time.timeSinceLevelLoad < GloballyImmuneUntil;

        public void MakeGloballyImmune(float duration) => GloballyImmuneUntil = Time.timeSinceLevelLoad + duration;

        public bool IsLocallyImmune(int id) =>
            _locallyImmuneUntil.TryGetValue(id, out var data)
            && (IsTriggerProcessActive ? data.Value : Time.timeSinceLevelLoad < data.Key);

        public void MakeLocallyImmune(int id, float duration) =>
            _locallyImmuneUntil[id] = new KeyValuePair<float, bool>(Time.timeSinceLevelLoad + duration, false);

        public static bool IsImmune(GameObject obj, TriggerListener listener) =>
            obj.TryGetComponent(out TeleporterImmunityStorage storage)
            && (storage.IsGloballyImmune || storage.IsLocallyImmune(listener.GetInstanceID()));

    }

}

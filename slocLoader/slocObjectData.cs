using System;
using Mirror;
using UnityEngine;

namespace slocLoader {

    [DisallowMultipleComponent]
    public sealed class slocObjectData : MonoBehaviour {

        [NonSerialized]
        public NetworkIdentity networkIdentity;

        public uint netId => networkIdentity.netId;

        public bool HasColliderOnClient { get; internal set; } = true;

        public bool ShouldBeSpawnedOnClient { get; internal set; } = true;

        private void Awake() {
            networkIdentity = GetComponent<NetworkIdentity>();
        }

    }

}

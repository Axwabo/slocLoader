using System;
using Mirror;
using UnityEngine;

namespace slocLoader {

    [DisallowMultipleComponent]
    public class slocSpawnedObject : MonoBehaviour {

        [NonSerialized] public NetworkIdentity networkIdentity;

        public uint netId => networkIdentity.netId;

        private void Awake() {
            networkIdentity = GetComponent<NetworkIdentity>();
        }

    }

}

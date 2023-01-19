using System;
using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    [Serializable]
    public sealed class RuntimeTeleportToSpawnedObjectData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.Toy;
        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        public GameObject go;

        public Vector3 offset;

        public RuntimeTeleportToSpawnedObjectData(GameObject go, Vector3 offset) {
            this.go = go;
            this.offset = offset;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.Write(go.GetInstanceID());
            writer.WriteVector(offset);
        }

    }

}

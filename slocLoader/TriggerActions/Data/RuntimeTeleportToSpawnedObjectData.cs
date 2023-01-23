using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class RuntimeTeleportToSpawnedObjectData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        [field: SerializeField]
        public GameObject Target { get; set; }

        [field: SerializeField]
        public Vector3 Offset { get; set; }

        public RuntimeTeleportToSpawnedObjectData(GameObject target, Vector3 offset) {
            Target = target;
            Offset = offset;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.Write(Target.GetInstanceID());
            writer.WriteVector(Offset);
        }

    }

}

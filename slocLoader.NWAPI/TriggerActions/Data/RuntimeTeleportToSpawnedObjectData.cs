using System.IO;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class RuntimeTeleportToSpawnedObjectData : BaseTeleportData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        [field: SerializeField]
        public GameObject Target { get; set; }

        public RuntimeTeleportToSpawnedObjectData(GameObject target, Vector3 offset) {
            Target = target;
            Position = offset;
        }

        protected override void WriteAdditionalData(BinaryWriter writer) => writer.Write(Target.GetInstanceID());

    }

}

using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class SerializableTeleportToSpawnedObjectData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        public readonly int ID;

        public readonly Vector3 Offset;

        public SerializableTeleportToSpawnedObjectData(int id, Vector3 offset) {
            ID = id;
            Offset = offset;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.Write(ID);
            writer.WriteVector(Offset);
        }

    }

}

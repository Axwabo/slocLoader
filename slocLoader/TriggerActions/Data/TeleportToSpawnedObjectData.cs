using System.IO;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class TeleportToSpawnedObjectData : BaseTriggerActionData {

        public override TargetType TargetType => TargetType.Toy;
        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        public readonly int ID;

        public readonly Vector3 Offset;

        public TeleportToSpawnedObjectData(int id, Vector3 offset) {
            ID = id;
            Offset = offset;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.Write(ID);
            writer.WriteVector(Offset);
        }

    }

}

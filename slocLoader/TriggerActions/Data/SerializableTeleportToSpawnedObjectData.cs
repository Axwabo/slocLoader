using System.IO;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public sealed class SerializableTeleportToSpawnedObjectData : BaseTriggerActionData {

        public override TargetType PossibleTargets => TargetType.All;

        public override TriggerActionType ActionType => TriggerActionType.TeleportToSpawnedObject;

        public readonly int ID;

        public readonly Vector3 Offset;

        public readonly TeleportOptions Options;

        public SerializableTeleportToSpawnedObjectData(int id, Vector3 offset, TeleportOptions options) {
            ID = id;
            Offset = offset;
            Options = options;
        }

        protected override void WriteData(BinaryWriter writer) {
            writer.Write(ID);
            writer.WriteVector(Offset);
            writer.Write((byte) Options);
        }

    }

}

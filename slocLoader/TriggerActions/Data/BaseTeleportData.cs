using System.IO;
using slocLoader.TriggerActions.Enums;
using UnityEngine;

namespace slocLoader.TriggerActions.Data {

    public abstract class BaseTeleportData : BaseTriggerActionData {

        public Vector3 Position { get; set; }

        [field: SerializeField]
        public TeleportOptions Options { get; set; }

        protected sealed override void WriteData(BinaryWriter writer) {
            writer.WriteVector(Position);
            writer.Write((byte) Options);
            WriteAdditionalData(writer);
        }

        protected virtual void WriteAdditionalData(BinaryWriter writer) {
        }

        public Vector3 ToWorldSpacePosition(Transform reference) =>
            Options.HasFlag(TeleportOptions.WorldSpaceTransform)
                ? reference.position + Position
                : reference.TransformPoint(Position);

    }

}

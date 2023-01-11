using System.IO;
using UnityEngine;

namespace slocLoader.Objects {

    public sealed class slocTransform {

        public Vector3 Position = Vector3.zero;
        public Vector3 Scale = Vector3.one;
        public Quaternion Rotation = Quaternion.identity;

        public void WriteTo(BinaryWriter writer) {
            writer.WriteVector(Position);
            writer.WriteVector(Scale);
            writer.WriteQuaternion(Rotation);
        }

        public static implicit operator slocTransform(Transform transform) => new() {
            Position = transform.localPosition,
            Scale = transform.localScale,
            Rotation = transform.localRotation
        };

    }

}

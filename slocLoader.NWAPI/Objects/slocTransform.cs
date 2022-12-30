using System.IO;
using UnityEngine;

namespace slocLoader.Objects {

    public sealed class slocTransform {

        public Vector3 Position = Vector3.zero;
        public Vector3 Scale = Vector3.one;
        public Quaternion Rotation = Quaternion.identity;

        public void WriteTo(BinaryWriter writer) {
            writer.Write(Position.x);
            writer.Write(Position.y);
            writer.Write(Position.z);
            writer.Write(Scale.x);
            writer.Write(Scale.y);
            writer.Write(Scale.z);
            writer.Write(Rotation.x);
            writer.Write(Rotation.y);
            writer.Write(Rotation.z);
            writer.Write(Rotation.w);
        }

        public static implicit operator slocTransform(Transform transform) => new slocTransform {
            Position = transform.localPosition,
            Scale = transform.localScale,
            Rotation = transform.localRotation
        };

    }

}

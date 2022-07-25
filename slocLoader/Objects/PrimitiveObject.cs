using System;
using System.IO;
using UnityEngine;

namespace slocLoader.Objects {

    public class PrimitiveObject : slocGameObject {

        public PrimitiveObject(int instanceId, ObjectType type) : base(instanceId) {
            if (type is ObjectType.None or ObjectType.Light)
                throw new ArgumentException("Invalid primitive type", nameof(type));
            Type = type;
        }

        public Color MaterialColor;

        public override void WriteTo(BinaryWriter writer) {
            base.WriteTo(writer);
            writer.Write(MaterialColor.r);
            writer.Write(MaterialColor.g);
            writer.Write(MaterialColor.b);
            writer.Write(MaterialColor.a);
        }

    }

}

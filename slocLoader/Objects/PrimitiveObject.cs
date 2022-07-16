using System;
using System.IO;
using UnityEngine;

namespace slocLoader.Objects {

    public class PrimitiveObject : slocGameObject {

        public PrimitiveObject(ObjectType type) {
            if (type is ObjectType.None || type is ObjectType.Light)
                throw new ArgumentException("PrimitiveObject cannot be of type None or Light");
            Type = type;
        }

        public Color MaterialColor = Color.gray;

        public override void WriteTo(BinaryWriter writer) {
            base.WriteTo(writer);
            writer.Write(MaterialColor.r);
            writer.Write(MaterialColor.g);
            writer.Write(MaterialColor.b);
            writer.Write(MaterialColor.a);
        }

    }

}

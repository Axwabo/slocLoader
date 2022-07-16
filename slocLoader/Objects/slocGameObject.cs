using System.IO;

namespace slocLoader.Objects {

    public abstract class slocGameObject {

        public ObjectType Type { get; protected set; } = ObjectType.None;
        public slocTransform Transform = new();

        public override string ToString() => $"{Type};{Transform}";

        public bool IsEmpty => Type == ObjectType.None;

        public virtual void WriteTo(BinaryWriter writer) {
            writer.Write((byte) Type);
            Transform.WriteTo(writer);
        }

    }

}

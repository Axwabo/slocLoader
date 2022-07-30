using System.IO;

namespace slocLoader.Objects {

    public abstract class slocGameObject {

        protected slocGameObject(int instanceId) => InstanceId = instanceId;

        public readonly int InstanceId;

        public int ParentId = 0;

        public bool HasParent => ParentId != InstanceId;

        public ObjectType Type { get; protected set; } = ObjectType.None;
        public slocTransform Transform = new slocTransform();

        public virtual bool IsValid => Type != ObjectType.None;

        public virtual void WriteTo(BinaryWriter writer) {
            writer.Write((byte) Type);
            writer.Write(InstanceId);
            writer.Write(ParentId);
            Transform.WriteTo(writer);
        }

    }

}

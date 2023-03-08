namespace slocLoader.Objects {

    public sealed class EmptyObject : slocGameObject {

        public EmptyObject() : this(0) {
        }

        public EmptyObject(int instanceId) : base(instanceId) => Type = ObjectType.Empty;

        public override bool IsValid => true;

    }

}

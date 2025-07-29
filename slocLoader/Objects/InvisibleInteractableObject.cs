using AdminToys;
using slocLoader.Extensions;
using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class InvisibleInteractableObject : slocGameObject
{

    public InvisibleInteractableObject(InvisibleInteractableToy.ColliderShape shape, int instanceId = 0)
        : base(instanceId)
    {
        Type = ObjectType.InvisibleInteractable;
        Shape = shape;
    }

    public InvisibleInteractableToy.ColliderShape Shape;

    public bool Locked;

    public float InteractionDuration;

    public override bool IsValid => true;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.WriteByteWithBool((byte) Shape, Locked);
        writer.Write(InteractionDuration);
    }

}

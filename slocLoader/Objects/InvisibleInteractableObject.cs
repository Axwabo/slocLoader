using AdminToys;
using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class InvisibleInteractableObject : slocGameObject
{

    public InvisibleInteractableObject(InvisibleInteractableToy.ColliderShape shape, int instanceId = 0)
        : base(instanceId)
        => Shape = shape;

    public InvisibleInteractableToy.ColliderShape Shape;

    public float InteractionDuration;

    public override bool IsValid => true;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.Write((byte) Shape);
        writer.Write(InteractionDuration);
    }

}

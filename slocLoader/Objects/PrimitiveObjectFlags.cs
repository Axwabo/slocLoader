namespace slocLoader.Objects;

[Flags]
public enum PrimitiveObjectFlags : byte
{

    None = 0,
    Visible = 1,
    ClientCollider = 2,
    ServerCollider = 4,
    Trigger = 8,
    NotSpawned = 16

}

using slocLoader.Extensions;
using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class StructureObject : slocGameObject
{

    public StructureObject(StructureType structureType) : this(0, structureType)
    {
    }

    public StructureObject(int instanceId, StructureType structureType) : base(instanceId)
    {
        Type = ObjectType.Structure;
        Structure = structureType;
    }

    public StructureType Structure { get; set; }

    public bool RemoveDefaultLoot { get; set; }

    public override bool IsValid => Structure != StructureType.None;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
        => writer.WriteByteWithBool((byte) Structure, RemoveDefaultLoot);

    public enum StructureType : byte
    {

        None = 0,
        Adrenaline = 1,
        BinaryTarget = 2,
        DboyTarget = 3,
        EzBreakableDoor = 4,
        Generator = 5,
        HczBreakableDoor = 6,
        LczBreakableDoor = 7,
        LargeGunLocker = 8,
        MiscellaneousLocker = 9,
        Medkit = 10,
        RifleRack = 11,
        Scp018Pedestal = 12,
        Scp207Pedestal = 13,
        Scp244Pedestal = 14,
        Scp268Pedestal = 15,
        Scp500Pedestal = 16,
        Scp1576Pedestal = 17,
        Scp1853Pedestal = 18,
        Scp2176Pedestal = 19,
        SportTarget = 20,
        Workstation = 21

    }

}

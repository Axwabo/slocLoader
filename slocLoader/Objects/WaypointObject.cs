using slocLoader.Readers;

namespace slocLoader.Objects;

public sealed class WaypointObject : slocGameObject
{

    public WaypointObject(int instanceId = 0)
        : base(instanceId)
        => Type = ObjectType.Waypoint;

    public float Priority;

    public bool VisualizeBounds;

    protected override void WriteData(BinaryWriter writer, slocHeader header)
    {
        writer.Write(Priority);
        writer.Write(VisualizeBounds);
    }

}

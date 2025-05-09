using slocLoader.Objects;

namespace slocLoader.Extensions;

public static class Scp079CameraPrefabs
{

    public static IReadOnlyDictionary<Scp079CameraType, uint> CameraTypeToPrefabId { get; } = new Dictionary<Scp079CameraType, uint>
    {
        {Scp079CameraType.LightContainmentZone, 2026969629},
        {Scp079CameraType.HeavyContainmentZone, 144958943},
        {Scp079CameraType.EntranceZone, 3375932423},
        {Scp079CameraType.EntranceZoneArm, 1824808402},
        {Scp079CameraType.SurfaceZone, 1734743361}
    };

    public static IReadOnlyDictionary<uint, Scp079CameraType> PrefabIdToCameraType { get; }
        = CameraTypeToPrefabId.ToDictionary(k => k.Value, v => v.Key);

}

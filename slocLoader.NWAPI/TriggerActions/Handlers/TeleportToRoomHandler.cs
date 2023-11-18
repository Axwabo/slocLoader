using Axwabo.Helpers.Config;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToRoomHandler : TeleportHandlerBase<TeleportToRoomData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

    protected override bool TryCalculateTransform(TeleportToRoomData data, out Vector3 position, out Quaternion rotation)
    {
        var transform = ConfigHelper.GetRoomByRoomName(data.Room).SafeGetTransform();
        if (!transform)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = data.Options.HasFlagFast(TeleportOptions.WorldSpaceTransform) ? transform.position + data.Position : transform.TransformPoint(data.Position);
        var quaternion = Quaternion.Euler(0, data.RotationY, 0);
        rotation = data.Options.HasFlagFast(TeleportOptions.UseDeltaPlayerRotation) ? quaternion : transform.rotation * quaternion;
        return true;
    }

}

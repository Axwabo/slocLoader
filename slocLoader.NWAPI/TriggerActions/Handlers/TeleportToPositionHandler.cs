using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToPositionHandler : TeleportHandlerBase<TeleportToPositionData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

    protected override bool TryCalculateTransform(TeleportToPositionData data, out Vector3 position, out Quaternion rotation)
    {
        position = data.Position;
        rotation = Quaternion.Euler(0, data.RotationY, 0);
        return true;
    }

}

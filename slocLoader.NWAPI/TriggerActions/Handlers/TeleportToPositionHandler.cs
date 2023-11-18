using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToPositionHandler : TeleportHandlerBase<TeleportToPositionData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToPosition;

    protected override Transform GetReferenceTransform(Component component, TeleportToPositionData data) => component.transform;

}

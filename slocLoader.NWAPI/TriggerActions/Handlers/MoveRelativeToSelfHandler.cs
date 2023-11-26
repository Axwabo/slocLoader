using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class MoveRelativeToSelfHandler : TeleportHandlerBase<MoveRelativeToSelfData>
{

    public override TriggerActionType ActionType => TriggerActionType.MoveRelativeToSelf;

    protected override Transform GetReferenceTransform(Component component, MoveRelativeToSelfData data) => component.transform;

}

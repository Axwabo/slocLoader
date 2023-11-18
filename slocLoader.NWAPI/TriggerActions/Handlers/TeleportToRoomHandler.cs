using Axwabo.Helpers.Config;
using slocLoader.TriggerActions.Data;
using slocLoader.TriggerActions.Enums;
using slocLoader.TriggerActions.Handlers.Abstract;

namespace slocLoader.TriggerActions.Handlers;

public sealed class TeleportToRoomHandler : TeleportHandlerBase<TeleportToRoomData>
{

    public override TriggerActionType ActionType => TriggerActionType.TeleportToRoom;

    protected override Transform GetReferenceTransform(Component component, TeleportToRoomData data)
        => ConfigHelper.GetRoomByRoomName(data.Room).SafeGetTransform();

}

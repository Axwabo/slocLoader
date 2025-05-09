using CommandSystem;
using Mirror;
using slocLoader.Objects;

namespace slocLoader.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class StructureCommand : ICommand, IUsageProvider
{

    public string[] Usage { get; } = ["type"];
    public string Command => "sl_structure";
    public string[] Aliases => [];
    public string Description => "Spawns the specified structure type.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var p = Player.Get(sender);
        if (p == null)
        {
            response = "You must be a player to execute this command!";
            return false;
        }

#if EXILED
        if (!p.CheckPermission("sloc.spawn"))
        {
            response = "You don't have permission to do that (sloc.spawn)!";
#else
        if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
        {
            response = "You don't have permission to do that (FacilityManagement)!";
#endif
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "Usage: sl_structure <structure type>";
            return false;
        }

        if (!arguments.ParseEnumIgnoreCase(out StructureObject.StructureType type))
        {
            response = "Unknown structure type.\nValid types: " + string.Join(", ", Enum.GetNames(typeof(StructureObject.StructureType)));
            return false;
        }

        var position = PositionCommand.Defined.TryGetValue(p.UserId, out var pos) ? pos : p.Position;
        var rotation = RotationCommand.Defined.TryGetValue(p.UserId, out var rot)
            ? rot
            : new Quaternion(0, p.ReferenceHub.PlayerCameraReference.rotation.y, 0, 1);
        var go = new StructureObject(type)
        {
            Transform =
            {
                Position = position,
                Rotation = rotation
            }
        }.SpawnObject();
        response = $"Spawned {type} with NetID: {go.GetComponent<NetworkIdentity>().netId}";
        return true;
    }

}

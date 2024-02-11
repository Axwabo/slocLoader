using CommandSystem;
using Mirror;

namespace slocLoader.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public sealed class DestroyCommand : ICommand, IUsageProvider
{

    public string[] Usage { get; } = {"netID"};
    public string Command => "sl_destroy";
    public string[] Aliases { get; } = {"sl_del", "sl_d", "sl_remove"};
    public string Description => "Destroys a previously spawned object.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
#if EXILED
        if (!sender.CheckPermission("sloc.destroy"))
        {
            response = "You don't have permission to do that (sloc.destroy)!";
#else
        if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
        {
            response = "You don't have permission to do that (FacilityManagement)!";
#endif
            return false;
        }

        if (arguments.Count < 1)
        {
            response = "Usage: sl_destroy <netID>";
            return false;
        }

        if (!uint.TryParse(arguments.At(0), out var id))
        {
            response = "That is not a valid netID!";
            return false;
        }

        if (!NetworkServer.spawned.TryGetValue(id, out var obj) || !obj.TryGetComponent(out slocObjectData o))
        {
            response = "That is not a valid sloc netID!";
            return false;
        }

        NetworkServer.Destroy(o.gameObject);
        response = "Object destroyed!";
        return true;
    }

}

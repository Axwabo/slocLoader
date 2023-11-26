using CommandSystem;
using slocLoader.AutoObjectLoader;

namespace slocLoader.Commands;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class ReloadCommand : ICommand
{

    public string Command => "sl_reload";
    public string[] Aliases { get; } = {"sl_rl"};
    public string Description => "Reloads sloc objects from AppData.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(PlayerPermissions.ServerConfigs))
        {
            response = "You don't have permission to do that (ServerConfigs)!";
            return false;
        }

        AutomaticObjectLoader.LoadObjects();
        response = "Reload complete.";
        return true;
    }

}

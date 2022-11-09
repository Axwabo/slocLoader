using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using Mirror;

namespace slocLoader.Commands {

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public sealed class DestroyCommand : ICommand, IUsageProvider {

        public string[] Usage { get; } = {"sl_destroy <netID>"};

        public string Command => "sl_destroy";
        public string[] Aliases { get; } = {"sl_del", "sl_d", "sl_remove"};
        public string Description => "Destroys a previously spawned object.";


        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            if (!sender.CheckPermission("sloc.spawn")) {
                response = "You don't have permission to do that (sloc.spawn)!";
                return false;
            }

            if (arguments.Count < 1) {
                response = "Usage: sl_destroy <netID>";
                return false;
            }

            if (!uint.TryParse(arguments.At(0), out var id)) {
                response = "That is not a valid netID!";
                return false;
            }

            if (!NetworkIdentity.spawned.TryGetValue(id, out var obj) || !obj.TryGetComponent(out slocSpawnedObject o)) {
                response = "That is not a valid sloc netID!";
                return false;
            }

            NetworkServer.Destroy(o.gameObject);
            response = "Object destroyed!";
            return true;
        }

    }

}

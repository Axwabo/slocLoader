using System;
using CommandSystem;
using Mirror;
using PluginAPI.Core;
using RemoteAdmin;
using slocLoader.AutoObjectLoader;
using UnityEngine;

namespace slocLoader.Commands {

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class SpawnCommand : ICommand, IUsageProvider {

        public string[] Usage { get; } = {"sl_spawn <name>"};

        public string Command => "sl_spawn";
        public string[] Aliases { get; } = {"sl_s"};
        public string Description => "Spawns the objects from a loaded sloc file.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response) {
            var p = Player.Get((sender as PlayerCommandSender)?.ReferenceHub);
            if (p == null) {
                response = "You must be a player to execute this command!";
                return false;
            }

            if (!sender.CheckPermission(PlayerPermissions.FacilityManagement)) {
                response = "You don't have permission to do that (FacilityManagement)!";
                return false;
            }

            if (arguments.Count < 1) {
                response = "Usage: sl_spawn <object name>";
                return false;
            }

            if (!AutomaticObjectLoader.TryGetObjects(string.Join(" ", arguments), true, out var objects)) {
                response = "Could not find any objects with that name.";
                return false;
            }

            var go = API.SpawnObjects(objects, out var spawned, p.Position, new Quaternion(0, p.ReferenceHub.PlayerCameraReference.rotation.y, 0, 1));
            response = $"Spawned {spawned} GameObjects. NetID: {go.GetComponent<NetworkIdentity>().netId}";
            return true;
        }

    }

}

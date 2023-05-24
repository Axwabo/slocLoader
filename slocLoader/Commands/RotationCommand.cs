using CommandSystem;
using RemoteAdmin;

namespace slocLoader.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class RotationCommand : ICommand, IUsageProvider
{

    public static readonly Dictionary<string, Quaternion> Defined = new();

    public string Command => "sl_rotation";
    public string[] Aliases { get; } = {"sl_rot"};
    public string Description => "Sets the rotation to spawn objects with.";
    public string[] Usage { get; } = {"x", "y", "z"};

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender ps)
        {
            response = "You must be a player to execute this command!";
            return false;
        }

        if (arguments.Count < 3)
        {
            Defined.Remove(ps.SenderId);
            response = "Objects will now be spawned the way you're facing (horizontal rotation only).";
            return true;
        }

        if (!float.TryParse(arguments.At(0), out var x) || !float.TryParse(arguments.At(1), out var y) || !float.TryParse(arguments.At(2), out var z))
        {
            response = "Invalid angles!";
            return false;
        }

        var rot = Quaternion.Euler(x, y, z);
        Defined[ps.SenderId] = rot;
        response = $"Objects will now be spawned with rotation {rot.eulerAngles}";
        return true;
    }

}

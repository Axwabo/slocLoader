using CommandSystem;
using RemoteAdmin;

namespace slocLoader.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class PositionCommand : ICommand, IUsageProvider
{

    public static readonly Dictionary<string, Vector3> Defined = new();

    public string Command => "sl_position";
    public string[] Aliases { get; } = {"sl_pos", "sl_p"};
    public string Description => "Sets the position to spawn objects at.";
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
            response = "Objects will now be spawned at the your position.";
            return true;
        }

        if (!float.TryParse(arguments.At(0), out var x) || !float.TryParse(arguments.At(1), out var y) || !float.TryParse(arguments.At(2), out var z))
        {
            response = "Invalid coordinates!";
            return false;
        }

        var pos = new Vector3(x, y, z);
        Defined[ps.SenderId] = pos;
        response = $"Objects will now be spawned at {pos}";
        return true;
    }

}

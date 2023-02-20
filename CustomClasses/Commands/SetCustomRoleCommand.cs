using System;
using CommandSystem;
using JetBrains.Annotations;
using PluginAPI.Core;

namespace Mistaken.CustomClasses.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SetCustomRoleCommand : ICommand, IUsageProvider
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 2)
            {
                response = "Usage: setcustomrole [Player] [ID of role]";
                return false;
            }

            var list = Utils.RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out var args);
            if (list.Count == 0)
            {
                response = "Player not found";
                return false;
            }

            if (!uint.TryParse(args[0], out uint roleID))
            {
                response = "Invalid role ID";
                return false;
            }
            if(!PluginMain.CustomClasses.ContainsKey(roleID))
            {
                response = "Role ID not found";
                return false;
            }
            Activator.CreateInstance(PluginMain.CustomClasses[roleID].type, new object[] { Player.Get(list[0]) });
            
            response = "Player set to custom role";
            return true;
        }

        public string Command => "setcustomrole";
        public string[] Aliases { get; } = new []{"scr"};
        public string[] Usage => new[] { "%player%","ID of role" };
        public string Description => "Sets a player to specified custom role";
    }
}
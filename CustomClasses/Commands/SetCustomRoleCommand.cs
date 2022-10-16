using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using JetBrains.Annotations;
using Mistaken.API.Commands;
using Mistaken.CustomClasses.API;

namespace Mistaken.CustomClasses.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [PublicAPI]
    public class SetCustomRoleCommand : IBetterCommand, IUsageProvider
    {
        public override string Command => "setcustomrole";
        public string[] Usage => new[] { "%player%","ID of role" };
        public override string Description => "Sets a player to specified custom role";

        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if(args.Length != 2)
            {
                return new[] { "Usage: setcustomrole [Player] [ID of role]" };
            }

            var list = GetPlayers(args[0]);
            if(list.Count == 0)
            {
                return new[] { "Player not found" };
            }
            if(!uint.TryParse(args[1], out uint roleID))
            {
                return new[] { "Invalid role ID" };
            }
            Activator.CreateInstance(PluginHandler.CustomClasses[roleID],new object[]{ list[0] });
            success = true;
            return new[] { "Player set to custom role" };
            
        }
    }
}
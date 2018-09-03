using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace LeaderBot.Commands
{
	public class ViewInfoCommands : ModuleBase
	{

        public ViewInfoCommands(){
        }

        [Command("help"), Summary("Get's a list of all the commands")]
        public async Task showCommands(){
            await ReplyAsync("`-help`, shows all available commands\n" +
                             "`-rolecount`, gets your current amount of roles\n" +
                             "`-missingroles-`, returns a list of roles the user does not currently have" +
                             "`-admin`\n" +
                             "\t`-admin giverole <user that receives role> <role to give>`, gives a role to a user\n" +
                             "\t`-admin createroles`, currently adds roles from json format, but will be changed later to have more input");
        }

        [Command("rolecount"), Summary("Gets the role count of the current user")]
		public async Task roleCount()
		{
			int amountOfRoles = 0;
			try
			{
				foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)
				{
					if (role.Name.Equals("@everyone")) continue; // exclude @everyone role
					amountOfRoles += 1;
					await Logger.Log(new LogMessage(LogSeverity.Verbose, GetType().Name + ".roleCount", Context.Message.Author.ToString() + " " + role.Name));
				}
				await ReplyAsync($"{Context.Message.Author} has {amountOfRoles} roles");

			}
			catch (Exception ex)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".roleCount", "Unexpected Exception", ex));
			}
		}

        //will change, POC
        //FIXME lol
        [Command("missingRoles"),Summary("Gives a list of currently not attained roles")]
        public async Task missingRoles(){
            List<SocketRole> allGuildRoles = new List<SocketRole>();
            foreach (SocketRole guildRoles in ((SocketGuild)Context.Guild).Roles){
                allGuildRoles.Add(guildRoles);
            }
            foreach (SocketRole userRole in ((SocketGuildUser)Context.Message.Author).Roles){
                if (allGuildRoles.Contains(userRole))
                    allGuildRoles.Remove(userRole);
            }
            foreach(var unobtainedRole in allGuildRoles){
                await ReplyAsync(unobtainedRole.ToString());
            }
        }
	}
}

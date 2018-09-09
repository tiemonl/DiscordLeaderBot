using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using LeaderBot;
using System.Text;

namespace LeaderBot {
	public class ViewInfoCommands : ModuleBase {
		public ViewInfoCommands() {
		}

		[Command("help"), Summary("Get's a list of all the commands")]
		public async Task showCommands() {
			await ReplyAsync("`-help`, shows all available commands\n" +
							 "`-rolecount`, gets your current amount of roles\n" +
							 "`-missingroles`, returns a list of roles the user does not currently have\n" +
							 "`-getroledesc <role name>`, returns the description of the role and how to obtain\n" +
							 "`-admin`\n" +
							 "\t`-admin giverole <user that receives role> <role to give>`, gives a role to a user\n" +
							 "\t`-admin createroles`, currently adds roles from json format, but will be changed later to have more input");
		}

		[Command("rolecount"), Summary("Gets the role count of the current user")]
		public async Task roleCount([Summary("user to get role of. Defaults to user who sent the message if no user is specified.")] SocketGuildUser user) {

			try {
				int amountOfRoles = user.Roles.Count - 1;
				await ReplyAsync($"{user} has {amountOfRoles} roles");

			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".roleCount", "Unexpected Exception", ex));
			}
		}

		[Command("leaderboardRole"), Summary("Gets the role leaderboard")]
		public async Task getRoleLeaderboard([Summary("Places to see on leaderboard")] int roleCount = 10) {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("```css\n");
			stringBuilder.Append("Username".PadRight(30) + "| # of roles\n");
			stringBuilder.Append("".PadRight(42, '-')+"\n");
			try {
				var allUsers = new Dictionary<SocketGuildUser, int>();

				foreach (var user in (await Context.Guild.GetUsersAsync())) {
					if (!user.IsBot)
						allUsers.Add(user as SocketGuildUser, ((SocketGuildUser)user).Roles.Count - 1);
				}
				if (allUsers.Count > 0) {
					var sortedDict = from entry in allUsers orderby entry.Value descending select entry;
					int i = 0;
					foreach (var entry in sortedDict) {
						stringBuilder.Append(entry.Key.ToString().PadRight(30) + "|\t" + entry.Value + "\n");
						++i;
						if (i >= roleCount) {
							break;
						}
					}
					stringBuilder.Append("```");
					await ReplyAsync($"{stringBuilder.ToString()}");
				} else {
					await ReplyAsync($".getRoleLeaderboard Unexpected Exception");
				}
			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getRoleLeaderboard", "Unexpected Exception", ex));
			}
		}

		//will change, POC
		//FIXME lol
		[Command("missingRoles"), Summary("Gives a list of currently not attained roles")]
		public async Task missingRoles() {
			List<SocketRole> allGuildRoles = new List<SocketRole>();
			foreach (SocketRole guildRoles in ((SocketGuild)Context.Guild).Roles) {
				allGuildRoles.Add(guildRoles);
			}
			foreach (SocketRole userRole in ((SocketGuildUser)Context.Message.Author).Roles) {
				if (allGuildRoles.Contains(userRole))
					allGuildRoles.Remove(userRole);
			}
			foreach (var unobtainedRole in allGuildRoles) {
				await ReplyAsync(unobtainedRole.ToString());
			}
		}

		[Command("getRoleDesc"), Summary("Returns role description")]
		public async Task getRoleDesc([Summary("The role to get the description for")] string roleName) {
			var selectedRole = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
			var allRoles = RoleCommands.allRoles;
			var role = allRoles.Find(x => x.Name.ToLower() == selectedRole.Name.ToLower());
			await ReplyAsync($"To get ***{role.Name}***\n\t-{role.Description}");
		}
	}
}

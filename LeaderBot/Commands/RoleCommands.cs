using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text;

namespace LeaderBot {
	[Group("admin")]
	public class RoleCommands : ModuleBase {
		public Random rand = new Random();

		public RoleCommands() {
		}

		//https://docs.stillu.cc/
		[Command("createRoles"), Summary("Creates a role in the guild")]
		public async Task createRoles() {
			List<string> currentGuildRoles = new List<string>();
			foreach (SocketRole guildRoles in ((SocketGuild)Context.Guild).Roles) {
				currentGuildRoles.Add(guildRoles.Name);
			}

			foreach (var role in SupportingMethods.LoadAllRolesFromServer()) {
				if (!currentGuildRoles.Contains(role.Name)) {
					var randColor = new Color(rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256));
					await Context.Guild.CreateRoleAsync(role.Name, GuildPermissions.None, randColor);
					await Logger.Log(new LogMessage(LogSeverity.Verbose, GetType().Name + ".createRoles", "Added role to server: " + role.Name));
					await ReplyAsync($"Added role: {role.Name}\nHow to get: {role.Description}");
				}
			}
		}

		[Command("updateUsers"), Summary("edit's user info in database")]
		public async Task updateUsers() {
			StringBuilder sb = new StringBuilder();
			foreach (var user in (await Context.Guild.GetUsersAsync())) {
				if (!user.IsBot) {
					var userInfo = SupportingMethods.getUserInformation(user.ToString());

					foreach (SocketRole userRole in ((SocketGuildUser)user).Roles) {
						if (!userInfo.Roles.Contains(userRole.ToString()))
							SupportingMethods.updateArray("name", user.ToString(), "roles", userRole.ToString());
					}
					sb.Append($"{user} updated.\n");
				}
			}
			await ReplyAsync(sb.ToString());
		}

		[Command("giveRole"), Summary("Adds role to specified user"), RequireUserPermission(GuildPermission.Administrator)]
		public async Task giveRole([Summary("The user to add role to")] SocketGuildUser user, [Summary("The role to add")] string roleName) {

			var userInfo = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
			SupportingMethods.updateArray("name", user.ToString(), "roles", role.ToString());
			await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".addRole", userInfo.ToString() + " added role " + role.ToString()));
			await (userInfo as IGuildUser).AddRoleAsync(role);
			await ReplyAsync($"{userInfo} now has {role}");
		}

		[Command("reorderRoles"), Summary("Reorders roles based on difficulty"), RequireUserPermission(GuildPermission.Administrator)]
		public async Task reorderRoles() {
			var allRoles = SupportingMethods.LoadAllRolesFromServer().OrderBy(x => x.Difficulty).Select(x => x.Name).ToList();
			var allGuildRoles = Context.Guild.Roles.OrderBy(y => allRoles.IndexOf(y.Name)).ToList();

			//reorder leaderbot roles to be at the top for hierarchy purposes
			var leaderbotRoles = allGuildRoles.Where(x => x.Name.Contains("leaderbot")).Reverse().ToList();
			allGuildRoles.RemoveAll(x => x.Name.Contains("leaderbot"));
			allGuildRoles.AddRange(leaderbotRoles);

			//sort the list based on the difficulty
			var sorting = allGuildRoles.Select((role, pos) => {
				var res = new ReorderRoleProperties(role.Id, pos);
				return res;
			});
			await Context.Guild.ReorderRolesAsync(sorting);
			await ReplyAsync($"Roles have been reordered");
		}

		[Command("makeRole"), Summary("Creates a new role in the server"), RequireUserPermission(GuildPermission.Administrator)]
		public async Task makeRoles(string name, string description, int difficulty) {
			SupportingMethods.createRoleInDatabase(name, description, difficulty);
			await ReplyAsync($"Role has been inserted into the database");
			await createRoles();
		}

		[Command("rolesList"), Summary("prints role list"), RequireUserPermission(GuildPermission.Administrator)]
		public async Task printRolesList() {
			var allRoles = SupportingMethods.LoadAllRolesFromServer();
			StringBuilder sb = new StringBuilder();
			foreach (var role in allRoles) {
				var embed = new EmbedBuilder();
				Random r = new Random();
				embed.WithTitle(role.Name);
				embed.AddField("Description", role.Description);
				embed.AddField("Difficulty", role.Difficulty);
				embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
				await ReplyAsync("", embed: embed);
			}

		}

	}
}

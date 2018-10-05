using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using MongoDB.Driver;

namespace LeaderBot {
	public class ViewInfoCommands : ModuleBase {
		SupportingMethods methods = new SupportingMethods();

		public ViewInfoCommands() {
		}

		[Command("help"), Summary("Get's a list of all the commands"), Alias("Commands", "Commandlist")]
		public async Task showCommands() {
			await ReplyAsync("Parameters: <required> [optional]\n" +
								"`-help`, shows all available commands\n" +
								"`-rolecount [User]`, gets your current amount of roles if no user is specified, otherwise returns rolecount of user specified.\n" +
								"`-leaderboard <board title> [number of entries shown]`, returns a leaderboard of users with the most roles. Defaults to the top 10 users.\n" +
								"`-missingroles`, returns a list of roles the user does not currently have\n" +
								"`-getroledesc <role name>`, returns the description of the role and how to obtain\n" +
								"`-getexperience [user]`, returns your total experience if no user is specified.\n" +
								"`-admin`\n" +
								"\t`-admin giverole <user> <role>`, gives a role to a user\n" +
								"\t`-admin createroles`, currently adds roles from json format, but will be changed later to have more input\n" +
								"\t`-admin updateroles`, updates database fields\n" +
								"\t`-admin reorderroles`, reorders roles in the server based on role difficulties.\n" +
								"`-getpoints [user]`, returns your total points if no user is specified.\n" +
								"`-bet <points> <coin side>`, bets your points on a coinflip.\n" +
								"`-dailypoints`, get daily points added to your inventory.");
		}

		[Command("rolecount"), Summary("Gets the role count of the current user")]
		public async Task roleCount([Summary("user to get role of. Defaults to user who sent the message if no user is specified.")] SocketGuildUser user = null) {

			try {
				if (user == null) {
					user = Context.Message.Author as SocketGuildUser;
				}
				int amountOfRoles = user.Roles.Count - 1;
				await ReplyAsync($"{user} has {amountOfRoles} roles");

			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".roleCount", "Unexpected Exception", ex));
			}
		}

		//will change, POC
		//FIXME lol
		[Command("missingRoles"), Summary("Gives a list of currently not attained roles")]
		public async Task missingRoles() {
			List<SocketRole> allGuildRoles = new List<SocketRole>();
			foreach (SocketRole guildRoles in ((SocketGuild) Context.Guild).Roles) {
				allGuildRoles.Add(guildRoles);
			}
			foreach (SocketRole userRole in ((SocketGuildUser) Context.Message.Author).Roles) {
				if (allGuildRoles.Contains(userRole))
					allGuildRoles.Remove(userRole);
			}
			string missingroles = "";
			foreach (var unobtainedRole in allGuildRoles.OrderBy(x => x.Name)) {
				missingroles += unobtainedRole.ToString() + ", ";
			}
			await ReplyAsync(missingroles);
		}

		[Command("getRoleDesc"), Summary("Returns role description")]
		public async Task getRoleDesc([Summary("The role to get the description for")] string roleName) {
			var selectedRole = Context.Guild.Roles.FirstOrDefault(x => SupportingMethods.stringEquals(x.Name, roleName));
			var allRoles = SupportingMethods.LoadAllRolesFromServer();
			var role = allRoles.Find(x => SupportingMethods.stringEquals(x.Name, selectedRole.Name));
			await ReplyAsync($"To get ***{role.Name}***\n\t-{role.Description}\n\t-Difficulty: {role.Difficulty}");
		}

		[Command("getExperience"), Summary("Returns user experience"), Alias("getexp")]
		public async Task getExperience([Summary("The user to get exp total from")] SocketGuildUser userName = null) {
			try {

				if (userName == null) {
					userName = ((SocketGuildUser) Context.Message.Author);
				}
				var user = userName as SocketUser;
				UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
				if (userInfo != null) {
					var currentExp = userInfo.Experience;
					var level = Math.Round(Math.Pow(currentExp, 1 / 1.3) / 100);
					await ReplyAsync($"{user} has {currentExp} experience and is level {level}");

				}
			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getExperience", "Unexpected Exception", ex));
			}
		}

		[Command("test"), Summary("Returns user experience")]
		public async Task test() {
			try {
				var userName = ((SocketGuildUser) Context.Message.Author);
				var date = DateTime.Parse(userName.JoinedAt.ToString());
				var today = DateTime.Now;
				var daysInServer = today - date;
				await ReplyAsync($"user joined {date}\nToday: {today}\nuser in server for {daysInServer.Days} days");
			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".getExperience", "Unexpected Exception", ex));
			}
		}

		[Command("createEquipment"), Summary("Returns user experience")]
		public async Task createEquipment(string name, int atk, int def, int cost, int levelReq) {
			try {
				SupportingMethods.createEquipmentInShop(name, atk, def, cost, levelReq);
				await ReplyAsync($"Equipment Created");
			} catch (Exception ex) {
				await Logger.Log(new LogMessage(LogSeverity.Error, GetType().Name + ".createEquiment", "Unexpected Exception", ex));
			}
		}
	}
}

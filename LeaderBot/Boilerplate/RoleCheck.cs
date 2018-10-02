using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace LeaderBot {
	public class RoleCheck {
		private static DiscordSocketClient client;

		public RoleCheck() {
		}

		public static void setUpClient(DiscordSocketClient myClient) {
			client = myClient;
		}

		public static async Task reactionCountRoles(ISocketMessageChannel channel, SocketReaction reaction, string user) {
			UserInfo userInfo = SupportingMethods.getUserInformation(user);
			if (userInfo != null) {
				if (userInfo.ReactionCount >= 250 && !doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Major reaction")) {
					await addRole(reaction.User.Value as SocketGuildUser, "Overreaction", channel.Id);
				} else if (userInfo.ReactionCount >= 100 && !doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactionary")) {
					await addRole(reaction.User.Value as SocketGuildUser, "Major reaction", channel.Id);
				} else if (userInfo.ReactionCount >= 50 && !doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactor")) {
					await addRole(reaction.User.Value as SocketGuildUser, "Reactionary", channel.Id);
				} else if (userInfo.ReactionCount >= 25) {
					await addRole(reaction.User.Value as SocketGuildUser, "Reactor", channel.Id);
				}
			} else {
				await createUserInDatabase(reaction.User.Value as SocketUser, channel.Id);
			}
		}

		public static async Task coinflipRoles(SocketGuildUser user, int bet, bool win, ulong channelID) {
			UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
			if (userInfo != null) {
				if (bet >= 5000) {
					if (win) {
						await addRole(user, "Chicken Dinner", channelID);
					} else {
						await addRole(user, "Oof", channelID);
					}
				}
				if (userInfo.WinCoinflipStreak >= 5) {
					await addRole(user, "Winner Winner", channelID);
				} else if (userInfo.LoseCoinflipStreak >= 5) {
					await addRole(user, "Bad Luck", channelID);
				}
			} else {
				await createUserInDatabase(user as SocketUser, channelID);
			}
		}

		public static async Task dateJoinedRoles(SocketUser user, ulong channelID) {
			string userName = user.ToString();
			var userGuild = user as SocketGuildUser;
			UserInfo userInfo = SupportingMethods.getUserInformation(userName);
			if (userInfo != null) {
				DateTime dateJoined = DateTime.Parse(userInfo.DateJoined);
				TimeSpan daysInServer = DateTime.Now - dateJoined;
				if (daysInServer.Days >= 365 && !doesUserHaveRole(userGuild, "Midlife Crisis")) {
					await addRole(user as SocketGuildUser, "Senior", channelID);
				} else if (daysInServer.Days >= 270 && !doesUserHaveRole(userGuild, "Adult")) {
					await addRole(user as SocketGuildUser, "Midlife Crisis", channelID);
				} else if (daysInServer.Days >= 180 && !doesUserHaveRole(userGuild, "Teen")) {
					await addRole(user as SocketGuildUser, "Adult", channelID);
				} else if (daysInServer.Days >= 90 && !doesUserHaveRole(userGuild, "Child")) {
					await addRole(user as SocketGuildUser, "Teen", channelID);
				} else if (daysInServer.Days >= 30) {
					await addRole(user as SocketGuildUser, "Child", channelID);
				}
			} else {
				await createUserInDatabase(user, channelID);
			}
		}
		public static async Task messageCountRoles(SocketUser user, ulong channelID) {
			string userName = user.ToString();
			UserInfo userInfo = SupportingMethods.getUserInformation(userName);
			if (userInfo != null) {
				if (userInfo.IsBetaTester) {
					await addRole(user as SocketGuildUser, "Beta Tester", channelID);
				}
				if (userInfo.NumberOfMessages >= 10000 && !doesUserHaveRole(user as SocketGuildUser, "I could write a novel")) {
					await addRole(user as SocketGuildUser, "I wrote a novel", channelID);
				} else if (userInfo.NumberOfMessages >= 1000 && !doesUserHaveRole(user as SocketGuildUser, "I'm liking this server")) {
					await addRole(user as SocketGuildUser, "I could write a novel", channelID);
				} else if (userInfo.NumberOfMessages >= 10 && !doesUserHaveRole(user as SocketGuildUser, "Hi and Welcome!")) {
					await addRole(user as SocketGuildUser, "I'm liking this server", channelID);
				} else if (userInfo.NumberOfMessages >= 1) {
					await addRole(user as SocketGuildUser, "Hi and Welcome!", channelID);
				}
			} else {
				await createUserInDatabase(user, channelID);
			}
		}

		public static async Task createUserInDatabase(SocketUser userName, ulong id) {
			SupportingMethods.createUserInDatabase(userName);
			await addRole(userName as SocketGuildUser, "Family", id);
		}

		public static async Task addRole(SocketGuildUser user, string roleName, ulong channelID) {
			var userName = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => SupportingMethods.stringEquals(x.Name, roleName));
			if (!user.Roles.Contains(role)) {
				await Logger.Log(new LogMessage(LogSeverity.Info, $"{typeof(SupportingMethods).Name}.addRole", $"{userName} has earned {roleName}"));
				await (user as IGuildUser).AddRoleAsync(role);
				SupportingMethods.updateArray("name", user.ToString(), "roles", role.ToString());
				var channelName = client.GetChannel(channelID) as IMessageChannel;
				await channelName.SendMessageAsync($"{userName} has earned **{role.Name}**");
			}
		}

		public static bool doesUserHaveRole(SocketGuildUser user, string roleName) {
			bool result = false;
			var role = user.Roles.FirstOrDefault(x => x.Name == roleName);
			if (user.Roles.Contains(role)) {
				result = true;
			}
			return result;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LeaderBot.Utils;

namespace LeaderBot {
	public class RoleUtils {
		private static DiscordSocketClient client;

		public RoleUtils() {
		}

		public static void SetUpClient(DiscordSocketClient myClient) {
			client = myClient;
		}

		public static async Task ReactionCountRoles(ISocketMessageChannel channel, SocketReaction reaction, SocketGuildUser user) {
			var userInfo = Util.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (userInfo.reactionCount >= 250 && doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Major reaction")) {
					await giveRoleToUser(reaction.User.Value as SocketGuildUser, "Overreaction", channel.Id);
				} else if (userInfo.reactionCount >= 100 && doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactionary")) {
					await giveRoleToUser(reaction.User.Value as SocketGuildUser, "Major reaction", channel.Id);
				} else if (userInfo.reactionCount >= 50 && doesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactor")) {
					await giveRoleToUser(reaction.User.Value as SocketGuildUser, "Reactionary", channel.Id);
				} else if (userInfo.reactionCount >= 25) {
					await giveRoleToUser(reaction.User.Value as SocketGuildUser, "Reactor", channel.Id);
				}
			} else {
				await createUserInDatabase(reaction.User.Value as SocketUser, channel.Id);
			}
		}

		public static async Task CoinflipRoles(SocketGuildUser user, int bet, bool win, ulong channelID) {
			UserInfo userInfo = Util.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (bet >= 5000) {
					if (win) {
						await giveRoleToUser(user, "Chicken Dinner", channelID);
					} else {
						await giveRoleToUser(user, "Oof", channelID);
					}
				}
				if (userInfo.winCoinflipStreak >= 5) {
					await giveRoleToUser(user, "Winner Winner", channelID);
				} else if (userInfo.loseCoinflipStreak >= 5) {
					await giveRoleToUser(user, "Bad Luck", channelID);
				}
			} else {
				await createUserInDatabase(user as SocketUser, channelID);
			}
		}

		public static async Task DateJoinedRoles(SocketUser user, ulong channelID) {
			var userGuild = user as SocketGuildUser;
			UserInfo userInfo = Util.GetUserInformation(user.Id);
			if (userInfo != null) {
				DateTime dateJoined = DateTime.Parse(userInfo.dateJoined);
				TimeSpan daysInServer = DateTime.Now - dateJoined;
				if (daysInServer.Days >= 365 && doesUserHaveRole(userGuild, "Midlife Crisis")) {
					await giveRoleToUser(user as SocketGuildUser, "Senior", channelID);
				} else if (daysInServer.Days >= 270 && doesUserHaveRole(userGuild, "Adult")) {
					await giveRoleToUser(user as SocketGuildUser, "Midlife Crisis", channelID);
				} else if (daysInServer.Days >= 180 && doesUserHaveRole(userGuild, "Teen")) {
					await giveRoleToUser(user as SocketGuildUser, "Adult", channelID);
				} else if (daysInServer.Days >= 90 && doesUserHaveRole(userGuild, "Child")) {
					await giveRoleToUser(user as SocketGuildUser, "Teen", channelID);
				} else if (daysInServer.Days >= 30) {
					await giveRoleToUser(user as SocketGuildUser, "Child", channelID);
				}
			} else {
				await createUserInDatabase(user, channelID);
			}
		}
		public static async Task MessageCountRoles(SocketUser user, ulong channelID) {
			UserInfo userInfo = Util.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (userInfo.isBetaTester) {
					await giveRoleToUser(user as SocketGuildUser, "Beta Tester", channelID);
				}
				if (userInfo.numberOfMessages >= 10000 && doesUserHaveRole(user as SocketGuildUser, "I could write a novel")) {
					await giveRoleToUser(user as SocketGuildUser, "I wrote a novel", channelID);
				} else if (userInfo.numberOfMessages >= 1000 && doesUserHaveRole(user as SocketGuildUser, "I'm liking this server")) {
					await giveRoleToUser(user as SocketGuildUser, "I could write a novel", channelID);
				} else if (userInfo.numberOfMessages >= 10 && doesUserHaveRole(user as SocketGuildUser, "Hi and Welcome!")) {
					await giveRoleToUser(user as SocketGuildUser, "I'm liking this server", channelID);
				} else if (userInfo.numberOfMessages >= 1) {
					await giveRoleToUser(user as SocketGuildUser, "Hi and Welcome!", channelID);
				}
			} else {
				await createUserInDatabase(user, channelID);
			}
		}

        public static async Task DailyPointsRoles(SocketUser user, ulong channelID, int minDailyPoints, int maxDailyPoints, int pointsEarned, int jackpot) {
           UserInfo userInfo = Util.GetUserInformation(user.Id);
            if (userInfo != null) {
                if (pointsEarned.Equals(jackpot)) {
                    await giveRoleToUser(user as SocketGuildUser, "Jackpot!", channelID);
                }
				//seperated jackpot from other roles in case jackpot is the same value as one below.
				if (pointsEarned.Equals(123)) {
                    await giveRoleToUser(user as SocketGuildUser, "Count von Count", channelID);
                } else if (pointsEarned.Equals(minDailyPoints)) {
                    await giveRoleToUser(user as SocketGuildUser, "einhundert", channelID);
                } else if (pointsEarned.Equals(maxDailyPoints)) {
                    await giveRoleToUser(user as SocketGuildUser, "zweihundertfünfzig", channelID);
                }
            } else {
                await createUserInDatabase(user, channelID);
            }
        }

        public static async Task createUserInDatabase(SocketUser userName, ulong id) {
			Util.CreateUserInDatabase(userName);
			await giveRoleToUser(userName as SocketGuildUser, "Family", id);
		}

		public static async Task removeRoleFromUser(IReadOnlyCollection<IGuildUser> guildUsers, string roleName) {
			foreach (var user in guildUsers) {
				var userName = user as SocketUser;
				var currentGuild = user.Guild as SocketGuild;
				var role = currentGuild.Roles.FirstOrDefault(x => Util.StringEquals(x.Name, roleName));
				if ((user as SocketGuildUser).Roles.Contains(role)) {
					await user.RemoveRoleAsync(role);
					break;
				}
			}
		}

		public static async Task giveRoleToUser(SocketGuildUser user, string roleName, ulong channelID) {
			var userName = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => Util.StringEquals(x.Name, roleName));
			if (!user.Roles.Contains(role)) {
				await Logger.Log(new LogMessage(LogSeverity.Info, $"{typeof(Util).Name}.addRole", $"{userName} has earned {roleName}"));
				await (user as IGuildUser).AddRoleAsync(role);
				Util.UpdateArray("name", user.ToString(), "roles", role.ToString());
				var channelName = client.GetChannel(channelID) as IMessageChannel;
				if(role.Name != "???")
					await channelName.SendMessageAsync($"{userName} has earned **{role.Name}**");
			}
		}

		public static bool doesUserHaveRole(SocketGuildUser user, string roleName) {
			bool result = false;
			var role = user.Roles.FirstOrDefault(x => Util.StringEquals(x.Name, roleName));
			if (user.Roles.Contains(role)) {
				result = true;
			}
			return result;
		}
	}
}

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
			var userInfo = ObjectUtils.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (userInfo.reactionCount >= 250 && DoesUserHaveRole(reaction.User.Value as SocketGuildUser, "Major reaction")) {
					await GiveRoleToUser(reaction.User.Value as SocketGuildUser, "Overreaction", channel.Id);
				} else if (userInfo.reactionCount >= 100 && DoesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactionary")) {
					await GiveRoleToUser(reaction.User.Value as SocketGuildUser, "Major reaction", channel.Id);
				} else if (userInfo.reactionCount >= 50 && DoesUserHaveRole(reaction.User.Value as SocketGuildUser, "Reactor")) {
					await GiveRoleToUser(reaction.User.Value as SocketGuildUser, "Reactionary", channel.Id);
				} else if (userInfo.reactionCount >= 25) {
					await GiveRoleToUser(reaction.User.Value as SocketGuildUser, "Reactor", channel.Id);
				}
			} else {
				await CreateUserInDatabase(reaction.User.Value as SocketUser, channel.Id);
			}
		}

		public static async Task CoinflipRoles(SocketGuildUser user, int bet, bool win, ulong channelID) {
			UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (bet >= 5000) {
					if (win) {
						await GiveRoleToUser(user, "Chicken Dinner", channelID);
					} else {
						await GiveRoleToUser(user, "Oof", channelID);
					}
				}
				if (userInfo.winCoinflipStreak >= 5) {
					await GiveRoleToUser(user, "Winner Winner", channelID);
				} else if (userInfo.loseCoinflipStreak >= 5) {
					await GiveRoleToUser(user, "Bad Luck", channelID);
				}
			} else {
				await CreateUserInDatabase(user as SocketUser, channelID);
			}
		}

		public static async Task DateJoinedRoles(SocketUser user, ulong channelID) {
			var userGuild = user as SocketGuildUser;
			UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
			if (userInfo != null) {
				DateTime dateJoined = DateTime.Parse(userInfo.dateJoined);
				TimeSpan daysInServer = DateTime.Now - dateJoined;
				if (daysInServer.Days >= 365 && DoesUserHaveRole(userGuild, "Midlife Crisis")) {
					await GiveRoleToUser(user as SocketGuildUser, "Senior", channelID);
				} else if (daysInServer.Days >= 270 && DoesUserHaveRole(userGuild, "Adult")) {
					await GiveRoleToUser(user as SocketGuildUser, "Midlife Crisis", channelID);
				} else if (daysInServer.Days >= 180 && DoesUserHaveRole(userGuild, "Teen")) {
					await GiveRoleToUser(user as SocketGuildUser, "Adult", channelID);
				} else if (daysInServer.Days >= 90 && DoesUserHaveRole(userGuild, "Child")) {
					await GiveRoleToUser(user as SocketGuildUser, "Teen", channelID);
				} else if (daysInServer.Days >= 30) {
					await GiveRoleToUser(user as SocketGuildUser, "Child", channelID);
				}
			} else {
				await CreateUserInDatabase(user, channelID);
			}
		}
		public static async Task MessageCountRoles(SocketUser user, ulong channelID) {
			UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
			if (userInfo != null) {
				if (userInfo.isBetaTester) {
					await GiveRoleToUser(user as SocketGuildUser, "Beta Tester", channelID);
				}
				if (userInfo.numberOfMessages >= 10000 && DoesUserHaveRole(user as SocketGuildUser, "I could write a novel")) {
					await GiveRoleToUser(user as SocketGuildUser, "I wrote a novel", channelID);
				} else if (userInfo.numberOfMessages >= 1000 && DoesUserHaveRole(user as SocketGuildUser, "I'm liking this server")) {
					await GiveRoleToUser(user as SocketGuildUser, "I could write a novel", channelID);
				} else if (userInfo.numberOfMessages >= 10 && DoesUserHaveRole(user as SocketGuildUser, "Hi and Welcome!")) {
					await GiveRoleToUser(user as SocketGuildUser, "I'm liking this server", channelID);
				} else if (userInfo.numberOfMessages >= 1) {
					await GiveRoleToUser(user as SocketGuildUser, "Hi and Welcome!", channelID);
				}
			} else {
				await CreateUserInDatabase(user, channelID);
			}
		}

        public static async Task DailyPointsRoles(SocketUser user, ulong channelID, int minDailyPoints, int maxDailyPoints, int pointsEarned, int jackpot) {
           UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            if (userInfo != null) {
                if (pointsEarned.Equals(jackpot)) {
                    await GiveRoleToUser(user as SocketGuildUser, "Jackpot!", channelID);
                }
				//seperated jackpot from other roles in case jackpot is the same value as one below.
				if (pointsEarned.Equals(123)) {
                    await GiveRoleToUser(user as SocketGuildUser, "Count von Count", channelID);
                } else if (pointsEarned.Equals(minDailyPoints)) {
                    await GiveRoleToUser(user as SocketGuildUser, "einhundert", channelID);
                } else if (pointsEarned.Equals(maxDailyPoints)) {
                    await GiveRoleToUser(user as SocketGuildUser, "zweihundertfünfzig", channelID);
                }
            } else {
                await CreateUserInDatabase(user, channelID);
            }
        }

        public static async Task CreateUserInDatabase(SocketUser userName, ulong id) {
			CreateObjectUtils.CreateUserInDatabase(userName);
			await GiveRoleToUser(userName as SocketGuildUser, "Family", id);
		}

		public static async Task RemoveRoleFromUser(IReadOnlyCollection<IGuildUser> guildUsers, string roleName) {
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

		public static async Task GiveRoleToUser(SocketGuildUser user, string roleName, ulong channelID) {
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

		public static bool DoesUserHaveRole(SocketGuildUser user, string roleName) {
			bool result = false;
			var role = user.Roles.FirstOrDefault(x => Util.StringEquals(x.Name, roleName));
			if (user.Roles.Contains(role)) {
				result = true;
			}
			return result;
		}
	}
}

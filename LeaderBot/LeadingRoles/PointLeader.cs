using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LeaderBot.Utils;

namespace LeaderBot.LeadingRoles {
	public class PointLeader {
		public PointLeader() {
		}
		public static async Task CheckForNewLeader(IReadOnlyCollection<IGuildUser> guildUsers, ulong channelID) {
			IGuildUser userWithMostPoints = null;
			long userPoints = 0;
			foreach (var user in guildUsers) {
				if (!user.IsBot) {
					UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
					if (userInfo != null) {
						long currentUserPoints = userInfo.points;
						if (currentUserPoints > userPoints) {
							userWithMostPoints = user;
							userPoints = currentUserPoints;
						}
					}
				}
			}
			var currentTorchHolder = userWithMostPoints as SocketGuildUser;
			var role = currentTorchHolder.Roles.FirstOrDefault(x => Util.StringEquals(x.Name, "Point Torch"));
			if (!currentTorchHolder.Roles.Contains(role)) {
				await RoleUtils.RemoveRoleFromUser(guildUsers, "Point Torch");
				await RoleUtils.GiveRoleToUser(userWithMostPoints as SocketGuildUser, "Point Torch", channelID);
			}
		}
	}
}

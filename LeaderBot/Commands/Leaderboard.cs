using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace LeaderBot.Commands {
	[Group("leaderboard")]
	public class Leaderboard : ModuleBase {

		public Leaderboard() {

		}

		[Command("role"), Summary("Gets the role leaderboard")]
		public async Task getRoleLeaderboard([Summary("Places to see on leaderboard")] int userCount = 10) {
			var guildUsers = await Context.Guild.GetUsersAsync();
			StringBuilder leaderboard = SupportingMethods.createLeaderboard("Roles", guildUsers, userCount);
			await ReplyAsync($"{leaderboard.ToString()}");
		}

		[Command("experience"), Summary("Gets the role leaderboard"), Alias("exp")]
		public async Task getExpLeaderboard([Summary("Places to see on leaderboard")] int userCount = 10) {
			var guildUsers = await Context.Guild.GetUsersAsync();
			StringBuilder leaderboard = SupportingMethods.createLeaderboard("Experience", guildUsers, userCount);
			await ReplyAsync($"{leaderboard.ToString()}");
		}
	}
}

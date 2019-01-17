using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LeaderBot.Commands {
	public class PointsCommands : ModuleBase {
		const int MIN_DAILY_POINTS = 100;
		const int MAX_DAILY_POINTS = 250;
		const int JACKPOT_MULTIPLIER = 10;
		public PointsCommands() {
		}

		[Command("dailyPoints"), Summary("Adds points to user")]
		public async Task dailyPoints() {
			Util.SetupMongoCollection("pointsReceived");
			string currentDate = DateTime.Now.ToString("yyyyMMdd");
			PointsReceived pointsReceived = Util.getPointsReceived("date", currentDate);
			var user = Context.Message.Author;
			List<string> users = pointsReceived.users.ToList();

			if (users.Contains(user.ToString())) {
				TimeSpan untilReset = DateTime.Today.AddDays(1) - DateTime.Now;
				Util.SetupMongoCollection("userData");
				await ReplyAsync($"{user} has already reclaimed the daily points.\nClaim points in {untilReset.Hours}h {untilReset.Minutes}m");
			} else {
				Util.updateArray("date", currentDate, "users", user.ToString());
				Util.SetupMongoCollection("userData");
				Random rand = new Random();
				var points = rand.Next(MIN_DAILY_POINTS, MAX_DAILY_POINTS + 1);
				var jackpot = rand.Next(MIN_DAILY_POINTS, MAX_DAILY_POINTS + 1);
				await RoleUtils.dailyPointsRoles(user as SocketGuildUser, Context.Message.Channel.Id, MIN_DAILY_POINTS, MAX_DAILY_POINTS, points, jackpot);
				if (points.Equals(jackpot)) {
					points *= JACKPOT_MULTIPLIER;
					await ReplyAsync($"{ user} has hit the __***JACKPOT!***__");
				}
				Util.updateDocument(user.Id, "points", points);

				await ReplyAsync($"{user} earned {points} points!");
			}

		}

		[Command("getPoints"), Summary("gets user total points")]
		public async Task getPoints([Summary("The user to get point total from")] SocketGuildUser userName = null) {
			if (userName == null) {
				userName = ((SocketGuildUser) Context.Message.Author);
			}
			var user = userName as SocketUser;
			UserInfo userInfo = Util.getUserInformation(user.Id);
			if (userInfo != null) {
				var currentPoints = userInfo.points;
				await ReplyAsync($"{user} has {currentPoints} points!");

			}
		}

		[Command("bet"), Summary("bet with user total points")]
		public async Task bet([Summary("Amount of points to bet")] int bettingPoints, [Summary("Side of coin picked.")] string coinSide) {
			var user = ((SocketGuildUser) Context.Message.Author);
			var userId = user.Id;
			bool win = false;
			var embed = new EmbedBuilder();
			embed.WithTitle("Coin Toss");
			UserInfo userInfo = Util.getUserInformation(user.Id);
			if (userInfo != null) {
				var currentPoints = userInfo.points;
				if (bettingPoints > currentPoints) {
					await ReplyAsync($"{user} has {currentPoints} points! You cannot bet {bettingPoints}!");
				} else if (bettingPoints < 50) {
					await ReplyAsync($"Minimum bet is 50 points");
				} else if (!(Util.stringEquals("heads", coinSide) || Util.stringEquals("tails", coinSide))) {
					await ReplyAsync($"Coin sides are ***heads*** or ***tails***.");
				} else {
					Random rand = new Random();
					var num = rand.Next(0, 2);
					string result = null;
					if (num == 0) {
						result = "heads";
						embed.WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/2006_Quarter_Proof.png/244px-2006_Quarter_Proof.png");
					} else {
						result = "tails";
						embed.WithThumbnailUrl("https://mbtskoudsalg.com/images/quarter-transparent-tail-1.png");
					}
					
					if (Util.stringEquals(result, coinSide)) {
						win = true;
						Util.updateDocument(userId, "loseCoinflipStreak", userInfo.loseCoinflipStreak * -1);
						Util.updateDocument(userId, "winCoinflipStreak", 1);
						Util.updateDocument(userId, "points", bettingPoints);
						embed.WithColor(Color.Green);
						embed.AddInlineField("Result", "Winner");
						embed.AddInlineField("Coin side", result);
						embed.AddInlineField("Winning streak", userInfo.winCoinflipStreak + 1);
						embed.AddInlineField("Total points", currentPoints + bettingPoints);
					} else {
						Util.updateDocument(userId, "winCoinflipStreak", userInfo.winCoinflipStreak * -1);
						Util.updateDocument(userId, "loseCoinflipStreak", 1);
						Util.updateDocument(userId, "points", bettingPoints * -1);
						embed.WithColor(Color.Red);
						embed.AddInlineField("Result", "Loser");
						embed.AddInlineField("Coin side", result);
						embed.AddInlineField("Losing streak", userInfo.loseCoinflipStreak + 1);
						embed.AddInlineField("Total points", currentPoints - bettingPoints);
					}
					await ReplyAsync("", embed: embed);
					await RoleUtils.coinflipRoles(user, bettingPoints, win, Context.Message.Channel.Id);
				}


			}
		}
	}
}

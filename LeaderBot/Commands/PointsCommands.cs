using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderBot.Utils;

namespace LeaderBot.Commands {
    public class PointsCommands : ModuleBase {
        const int MIN_DAILY_POINTS = 100;
        const int MAX_DAILY_POINTS = 250;
        const int JACKPOT_MULTIPLIER = 10;
        public PointsCommands() {
        }

        [Command("dailyPoints"), Summary("Adds points to user")]
        public async Task DailyPoints() {
            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            DailyPoints dailyPoints = ObjectUtils.GetDailyPoints("_id", currentDate);
            var user = Context.Message.Author;
            List<string> users = dailyPoints.users.ToList();

            if (users.Contains(user.ToString())) {
                TimeSpan untilReset = DateTime.Today.AddDays(1) - DateTime.Now;
                await ReplyAsync($"{user} has already reclaimed the daily points.\nClaim points in {untilReset.Hours}h {untilReset.Minutes}m");
            } else {
                Util.UpdateArray("_id", currentDate, "users", user.ToString(),"dailyPoints");
                Random rand = new Random();
                var points = rand.Next(MIN_DAILY_POINTS, MAX_DAILY_POINTS + 1);
                var jackpot = rand.Next(MIN_DAILY_POINTS, MAX_DAILY_POINTS + 1);
                await RoleUtils.DailyPointsRoles(user as SocketGuildUser, Context.Message.Channel.Id, MIN_DAILY_POINTS, MAX_DAILY_POINTS, points, jackpot);
                if (points.Equals(jackpot)) {
                    points *= JACKPOT_MULTIPLIER;
                    await ReplyAsync($"{user} has hit the __***JACKPOT!***__");
                }
                DatabaseUtils.IncrementDocument(user.Id, "points", points);
                await ReplyAsync($"{user} earned {points} points!");
            }

        }

        [Command("getPoints"), Summary("gets user total points")]
        public async Task GetPoints([Summary("The user to get point total from")] SocketGuildUser userName = null) {
            if (userName == null) {
                userName = ((SocketGuildUser)Context.Message.Author);
            }
            var user = userName as SocketUser;
            UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            if (userInfo != null) {
                var currentPoints = userInfo.points;
                await ReplyAsync($"{user} has {currentPoints} points!");

            }
        }

        [Command("bet"), Summary("bet with user total points")]
        public async Task Bet([Summary("Amount of points to bet")] int bettingPoints, [Summary("Side of coin picked.")] string coinSide) {
            var user = ((SocketGuildUser)Context.Message.Author);
            var userId = user.Id;
            bool win = false;
            var embed = new EmbedBuilder();
            embed.WithTitle("Coin Toss");
            UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            if (userInfo != null) {
                var currentPoints = userInfo.points;
                if (bettingPoints > currentPoints) {
                    await ReplyAsync($"{user} has {currentPoints} points! You cannot bet {bettingPoints}!");
                } else if (bettingPoints < 50) {
                    await ReplyAsync($"Minimum bet is 50 points");
                } else if (!(Util.StringEquals("heads", coinSide) || Util.StringEquals("tails", coinSide))) {
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

                    if (Util.StringEquals(result, coinSide)) {
                        win = true;
                        DatabaseUtils.DecrementDocument(userId, "loseCoinflipStreak", userInfo.loseCoinflipStreak);
                        DatabaseUtils.IncrementDocument(userId, "winCoinflipStreak", 1);
                        DatabaseUtils.IncrementDocument(userId, "points", bettingPoints);
                        embed.WithColor(Color.Green);
                        embed.AddField("Result", "Winner", true);
                        embed.AddField("Coin side", result, true);
                        embed.AddField("Winning streak", userInfo.winCoinflipStreak + 1, true);
                        embed.AddField("Total points", currentPoints + bettingPoints, true);
                    } else {
                        DatabaseUtils.DecrementDocument(userId, "winCoinflipStreak", userInfo.winCoinflipStreak);
                        DatabaseUtils.IncrementDocument(userId, "loseCoinflipStreak", 1);
                        DatabaseUtils.DecrementDocument(userId, "points", bettingPoints);
                        embed.WithColor(Color.Red);
                        embed.AddField("Result", "Loser", true);
                        embed.AddField("Coin side", result, true);
                        embed.AddField("Losing streak", userInfo.loseCoinflipStreak + 1, true);
                        embed.AddField("Total points", currentPoints - bettingPoints, true);
                    }
                    await ReplyAsync("", embed: embed.Build());
                    await RoleUtils.CoinflipRoles(user, bettingPoints, win, Context.Message.Channel.Id);
                }


            }
        }
    }
}
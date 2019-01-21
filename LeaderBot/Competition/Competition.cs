using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderBot.Utils;

namespace LeaderBot.Competition {
    [Group("competition"), Alias("comp")]
    public class Competition : ModuleBase {
        public Competition() {
        }

        [Command("enter"), Summary("enters the user in the competition")]
        public async Task enterCompetition() {
            DatabaseUtils.ChangeCollection("competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            UsersEntered userInCompetition = Util.GetUsersInCompetition("_id", user.Id);

            if (user.JoinedAt > DateTime.Today.AddDays(-7)) {
                await ReplyAsync($"You have joined the server too recently to enter!");
            } else if (userInCompetition != null) {
                await ReplyAsync($"You have already joined the competition!");
            } else {
                Util.CreateUserInCompetition(user);
                await ReplyAsync($"You have joined the competition, Good Luck!");
            }
            DatabaseUtils.ChangeCollection("userData");
        }

        [Command("rollunder"), Alias("bet")]
        public async Task rollUnder(int UserUnderValue, int bet) {
            DatabaseUtils.ChangeCollection("competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            var userId = user.Id;
            var embed = new EmbedBuilder();
            UsersEntered userInCompetition = Util.GetUsersInCompetition("_id", userId);
            if (UserUnderValue < 2 || UserUnderValue > 99) {
                await ReplyAsync($"Undervalue must be between 2 and 99");
            } else if (userInCompetition.credits < bet) {
                await ReplyAsync($"{user} has {userInCompetition.credits} credits! You cannot bet {bet}!");
            } else if (bet <= 0) {
                await ReplyAsync($"Bet must bet 1 or more credits");
            } else {

                Random rand = new Random(DateTime.Now.Millisecond);
                var value = rand.Next(100) + 1;
                double factor = 100 / (double)(UserUnderValue - 1);
                if (UserUnderValue > value) //win
                {
                    var winnings = (bet * factor) - bet;
                    Util.UpdateDocument(userId, "credits", (int)winnings);
                    embed.WithColor(Color.Green);
                    embed.AddField("Result", "Winner", true);
                    embed.AddField("Random Value", value, true);
                    embed.AddField("Winnings", (int)winnings, true);
                    embed.AddField("Total points", userInCompetition.credits + (int)winnings, true);
                } else {
                    Util.UpdateDocument(userId, "credits", -bet);
                    embed.WithColor(Color.Red);
                    embed.AddField("Result", "Loser", true);
                    embed.AddField("Random Value", value, true);
                    embed.AddField("Losing", -bet, true);
                    embed.AddField("Total points", userInCompetition.credits - bet, true);
                }
                await ReplyAsync("", embed: embed.Build());
            }

            DatabaseUtils.ChangeCollection("userData");
        }
    }
}

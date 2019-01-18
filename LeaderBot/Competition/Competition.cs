using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LeaderBot.Competition
{
    [Group("competition"), Alias("comp")]
    public class Competition : ModuleBase
    {
        public Competition()
        {
        }

        [Command("enter"), Summary("enters the user in the competition")]
        public async Task enterCompetition()
        {
            Util.SetupMongoCollection("competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            UsersEntered userInCompetition = Util.getUsersInCompetition("_id", user.Id);
            
            if (user.JoinedAt > DateTime.Today.AddDays(-7))
            {
                await ReplyAsync($"You have joined the server too recently to enter!");
            }
            else if (userInCompetition != null)
            {
                await ReplyAsync($"You have already joined the competition!");
            }
            else
            {
                Util.createUserInCompetition(user);
                await ReplyAsync($"You have joined the competition, Good Luck!");
            }
            Util.SetupMongoCollection("userData");
        }

        [Command("rollunder")]
        public async Task rollUnder(int UserUnderValue, int bet)
        {
            Util.SetupMongoCollection("competition");
            var user = ((SocketGuildUser)Context.Message.Author);
            var userId = user.Id;
            var embed = new EmbedBuilder();
            UsersEntered userInCompetition = Util.getUsersInCompetition("_id", userId);
            if (UserUnderValue < 2 || UserUnderValue > 99) {
                await ReplyAsync($"Undervalue must be between 2 and 99");
            } else if (userInCompetition.credits < bet)
            {
                await ReplyAsync($"{user} has {userInCompetition.credits} credits! You cannot bet {bet}!");
            } else if (bet <= 0)
            {
                await ReplyAsync($"Bet must bet 1 or more credits");
            }
            else
            {

                Random rand = new Random();
                var value = rand.Next(100)+1;
                double factor = 100 / (bet - 1);
                if (UserUnderValue > value) //win
                {
                    var winnings = bet * factor;
                    //double result = bet*(3 * (double)(chanceOfLoss / 100)); //higher chance of loss closer to 3x multiplier on winning
                    Util.updateDocument(userId, "credits", (int)winnings);
                    embed.WithColor(Color.Green);
                    embed.AddField("Result", "Winner", true);
                    embed.AddField("Random Value", value, true);
                    embed.AddField("Winnings", winnings, true);
                    embed.AddField("Total points", userInCompetition.credits + winnings, true);
                }
                else
                {
                    Util.updateDocument(userId, "credits", -bet);
                    embed.WithColor(Color.Red);
                    embed.AddField("Result", "Loser", true);
                    embed.AddField("Random Value", value, true);
                    embed.AddField("Losing", -bet, true);
                    embed.AddField("Total points", userInCompetition.credits - bet, true);
                }
                await ReplyAsync("", embed: embed.Build());
            }

            Util.SetupMongoCollection("userData");
        }
    }
}

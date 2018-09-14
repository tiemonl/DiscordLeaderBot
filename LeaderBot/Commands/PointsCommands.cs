using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LeaderBot.Points;

namespace LeaderBot.Commands
{
    public class PointsCommands : ModuleBase
    {
        public PointsCommands()
        {
        }

        [Command("dailyPoints"), Summary("Adds points to user")]
        public async Task dailyPoints()
        {
            SupportingMethods.SetupDatabase("pointsReceived");

            string currentDate = DateTime.Now.ToString("yyyyMMdd");
            PointsReceived pointsReceived = SupportingMethods.getPointsReceived("date", currentDate);
            var user = Context.Message.Author;
            List<string> users = pointsReceived.Users.ToList();

            if (users.Contains(user.ToString()))
            {
                await ReplyAsync($"{user} has already reclaimed the daily points.");
            }
            else
            {
                SupportingMethods.updateArray("date",currentDate, "users", user.ToString());
                SupportingMethods.SetupDatabase("userData");
                Random rand = new Random();
                var points = rand.Next(100, 250);
                SupportingMethods.updateDocument(user.ToString(), "points", points);

                await ReplyAsync($"{user} earned {points} points!");
            }

        }

        [Command("getPoints"), Summary("gets user total points")]
        public async Task getPoints([Summary("The user to get point total from")] SocketGuildUser userName = null)
        {
            if (userName == null)
            {
                userName = ((SocketGuildUser)Context.Message.Author);
            }
            var user = userName as SocketUser;
            UserInfo userInfo = SupportingMethods.getUserInformation(user.ToString());
            if (userInfo != null)
            {
                var currentPoints = userInfo.Points;
                await ReplyAsync($"{user} has {currentPoints} points!");

            }
        }

    }
}

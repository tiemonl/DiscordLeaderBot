using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using LeaderBot.Utils;

namespace LeaderBot.Commands
{
    [Group("bank")]
    public class PointBank : ModuleBase
    {
        public PointBank()
        {
        }

        [Command("points"), Summary("gets bank total points")]
        public async Task getPoints() {
            await ReplyAsync($"Bank has 1000 points!");

        }

        [Command("test"), Summary("gets bank total points")]
        public async Task test() {
            UserInfo userInfo = Util.GetUserInformation(Context.User.Id);
            DatabaseUtils.UpdateDocumentValue(Context.User.Id, "isBetaTester", true);

        }


    }
}

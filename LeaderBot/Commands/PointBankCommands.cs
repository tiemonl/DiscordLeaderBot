using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using LeaderBot.Utils;
using LeaderBot.Objects;

namespace LeaderBot.Commands
{
    [Group("bank")]
    public class PointBankCommands : ModuleBase {
        public PointBankCommands() {
        }

        [Command("points"), Summary("gets bank total points")]
        public async Task GetPoints() {
            await ReplyAsync($"Bank has 1000 points!");

        }

        [Command("info"), Summary("returns specified bank information")]
        public async Task GetBankInfo([Remainder] string bank) {
            EmbedBuilder embed = new EmbedBuilder();
            PointBank pointBank = Util.GetPointBank(bank.ToLower());
            if (pointBank.Equals(null)) {
                await ReplyAsync($"{bank} does not exist");
            } else {
                embed.Title = pointBank._id;
                embed.AddField("Money in vault", pointBank.currentCredits, true);
                embed.AddField("Interest Rate", $"{pointBank.interestRate * 100}%", true);
                embed.Color = Color.Purple;
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("takeloan"), Summary("gets bank total points")]
        public async Task TakeLoan(int amount, [Remainder] string bank) {
            UserInfo userInfo = Util.GetUserInformation(Context.User.Id);
            PointBank pointBank = Util.GetPointBank(bank.ToLower());
            EmbedBuilder embed = new EmbedBuilder();
            if (amount <= pointBank.minWithdrawal) {
                await ReplyAsync($"Cannot take out a loan less than {pointBank.minWithdrawal} points!");
            } else if (amount > pointBank.currentCredits){
                await ReplyAsync($"Cannot take out a loan which exceeds the amount of money in the vault!\nCurrent money in the vault is {pointBank.currentCredits}");
            } else {
                DatabaseUtils.IncrementDocument(Context.User.Id, "points", amount);
                DatabaseUtils.DecrementDocument(pointBank._id, "currentCredits", amount, "pointBank");
                embed.Title = "Succesful loan approval!";
                embed.Color = Color.Green;
                embed.AddField($"{Context.User.ToString()} points", userInfo.points + amount);
                embed.AddField($"{pointBank._id}'s vault value", pointBank.currentCredits - amount);
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}

using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using LeaderBot.Utils;
using LeaderBot.Objects;
using System.Collections.Generic;
using MongoDB.Bson;

namespace LeaderBot.Commands
{
    [Group("bank")]
    public class PointBankCommands : ModuleBase {
        public PointBankCommands() {
        }

        [Command("new"), Summary("gets bank total points")]
        public async Task GetPoints() {
            await ReplyAsync($"Bank created");

        }

        [Command("info"), Summary("returns specified bank information")]
        public async Task GetBankInfo([Remainder] string bank) {
            EmbedBuilder embed = new EmbedBuilder();
            PointBank pointBank = ObjectUtils.GetPointBank(bank.ToLower());
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
            var user = Context.User;
            UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            PointBank pointBank = ObjectUtils.GetPointBank(bank.ToLower());
            EmbedBuilder embed = new EmbedBuilder();
            if (amount < pointBank.minWithdrawal) {
                await ReplyAsync($"Cannot take out a loan less than {pointBank.minWithdrawal} points!");
            } else if (amount > pointBank.currentCredits){
                await ReplyAsync($"Cannot take out a loan which exceeds the amount of money in the vault!\nCurrent money in the vault is {pointBank.currentCredits}");
            } else {
                pointBank.currentLoans.Add(user.Id.ToString(), amount);
                var dict = new BsonDocument {
                    { user.Id.ToString(), amount}
                };
                Util.UpdateArray("_id", pointBank._id, "currentLoans", dict, pointBank, "pointBanks");
                DatabaseUtils.IncrementDocument(Context.User.Id, "points", amount);
                DatabaseUtils.DecrementDocument(pointBank._id, "currentCredits", amount, "pointBanks");
                embed.Title = "Succesful loan approval!";
                embed.Color = Color.Green;
                embed.AddField($"{Context.User.ToString()} points", userInfo.points + amount);
                embed.AddField($"{pointBank._id}'s vault value", pointBank.currentCredits - amount);
                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}

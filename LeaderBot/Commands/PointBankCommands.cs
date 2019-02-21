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
            if (Util.CheckIfUserHasLoan(user.Id) != null) {
                await ReplyAsync("You already have a loan out for this bank!");
                return;
            }
            if (amount < pointBank.minWithdrawal) {
                await ReplyAsync($"Cannot take out a loan less than {pointBank.minWithdrawal} points!");
            } else if (amount > pointBank.currentCredits) {
                await ReplyAsync($"Cannot take out a loan which exceeds the amount of money in the vault!\nCurrent money in the vault is {pointBank.currentCredits}");
            } else {
                int loanBalance = (int)(amount * (1 + pointBank.interestRate));
                pointBank.currentLoans.Add(user.Id.ToString(),loanBalance);
                var dict = new BsonDocument {
                    { user.Id.ToString(), loanBalance}
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

        [Command("loans"), Summary("Gives current user loan information")]
        public async Task CurrentLoans() {
            var loans = Util.CheckIfUserHasLoan(Context.User.Id);
            EmbedBuilder embed = new EmbedBuilder {
                Title = "Current loans",
                Color = Color.DarkRed
            };
            foreach (var loan in loans) {
                embed.AddField(loan.Key, loan.Value);
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("payloan"), Summary("Lets users pay off loans")]
        public async Task PayLoan(int amount, [Remainder]string bank) {
            var user = Context.User;
            UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
            PointBank pointBank = ObjectUtils.GetPointBank(bank.ToLower());
            var loans = Util.CheckIfUserHasLoan(Context.User.Id);
            foreach (var loan in loans) {
                if (bank.Equals(loan.Key)) {
                    if (userInfo.points < amount) {
                        await ReplyAsync($"{user.Username} has {userInfo.points}.\nYou cannot pay {amount}.");
                        return;
                    } else if (amount > loan.Value) {
                        await ReplyAsync($"Loan is {loan.Value}.\nYou cannot pay {amount}.");
                        return;
                    } else if (amount < 1) {
                        await ReplyAsync($"You cannot pay less than 1 point.");
                        return;
                    } else if (amount.Equals(loan.Value)) {
                        DatabaseUtils.DecrementDocument(user.Id, "points", amount);
                        pointBank.currentLoans.Remove(user.Id.ToString());
                        Util.UpdateArray("_id", pointBank._id, "currentLoans", user.Id.ToString(), pointBank, "pointBanks", false);
                        await ReplyAsync($"Loan paid back in full to {pointBank._id}");
                        return;
                    } else {
                        DatabaseUtils.DecrementDocument(user.Id, "points", amount);
                        pointBank.currentLoans[user.Id.ToString()] -= amount;
                        Util.UpdateArray("_id", pointBank._id, "currentLoans", user.Id.ToString(), pointBank, "pointBanks", false);
                        await ReplyAsync($"Payment on loan made.\nRemaining balance {pointBank.currentLoans[user.Id.ToString()]}");
                        return;
                    }
                }
            }
            await ReplyAsync("You do not have a loan out for this bank.");
        }
    }
}

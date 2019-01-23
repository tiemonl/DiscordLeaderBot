using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LeaderBot.Objects;
using Discord.WebSocket;
using LeaderBot.Utils;

namespace LeaderBot.Commands {
	[Group("shop")]
	public class ShopCommands : ModuleBase{
		public ShopCommands() {
		}
		[Command("view")]
		public async Task viewShop(int item) {
			DatabaseUtils.ChangeCollection("shop");
			Shop shopItem = Util.GetShopItem(item);
			var embed = new EmbedBuilder();
			Random r = new Random();
			embed.WithTitle(shopItem.name);
			embed.AddField("Attack", shopItem.attack, true);
			embed.AddField("Defence", shopItem.defence, true);
			embed.AddField("Cost", shopItem.cost, true);
			embed.AddField("Level Requirement", shopItem.levelRequirement, true);
			embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
			await ReplyAsync("", embed: embed.Build());
			DatabaseUtils.ChangeCollection("userData");
		}

		[Command("buy")]
		public async Task buyItemFromShop(int item) {
			var user = ((SocketGuildUser)Context.Message.Author);
			var userId = user.Id;
			DatabaseUtils.ChangeCollection("shop");
			Shop shopItem = Util.GetShopItem(item);
			DatabaseUtils.ChangeCollection("userData");
			UserInfo userInfo = Util.GetUserInformation(userId);
			var embed = new EmbedBuilder();
			Random r = new Random();
			if (userInfo.points < shopItem.cost) {
				await ReplyAsync($"User does not have enough points to buy item.");
			} else {
				DatabaseUtils.DecrementDocument(userId, "points", shopItem.cost);
                DatabaseUtils.IncrementDocument(userId, "totalAttack", shopItem.attack);
                DatabaseUtils.IncrementDocument(userId, "totalDefense", shopItem.defence);
				embed.WithTitle("Item bought!");
				embed.WithThumbnailUrl(user.GetAvatarUrl());
				embed.AddField("Attack", userInfo.totalAttack + shopItem.attack, true);
				embed.AddField("Defense", userInfo.totalDefense + shopItem.defence, true);
				embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
				await ReplyAsync("", embed: embed.Build());
			}
		}
	}
}

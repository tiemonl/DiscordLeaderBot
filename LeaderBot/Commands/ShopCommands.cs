using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LeaderBot.Objects;
using Discord.WebSocket;

namespace LeaderBot.Commands {
	[Group("shop")]
	public class ShopCommands : ModuleBase{
		public ShopCommands() {
		}
		[Command("view")]
		public async Task viewShop(int item) {
			Util.SetupMongoCollection("shop");
			Shop shopItem = Util.getShopItem(item);
			var embed = new EmbedBuilder();
			Random r = new Random();
			embed.WithTitle(shopItem.name);
			embed.AddInlineField("Attack", shopItem.attack);
			embed.AddInlineField("Defence", shopItem.defence);
			embed.AddInlineField("Cost", shopItem.cost);
			embed.AddInlineField("Level Requirement", shopItem.levelRequirement);
			embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
			await ReplyAsync("", embed: embed);
			Util.SetupMongoCollection("userData");
		}

		[Command("buy")]
		public async Task buyItemFromShop(int item) {
			var user = ((SocketGuildUser)Context.Message.Author);
			var userId = user.Id;
			Util.SetupMongoCollection("shop");
			Shop shopItem = Util.getShopItem(item);
			Util.SetupMongoCollection("userData");
			UserInfo userInfo = Util.getUserInformation(userId);
			var embed = new EmbedBuilder();
			Random r = new Random();
			if (userInfo.points < shopItem.cost) {
				await ReplyAsync($"User does not have enough points to buy item.");
			} else {
				Util.updateDocument(userId, "points", shopItem.cost*-1);
				Util.updateDocument(userId, "totalAttack", shopItem.attack);
				Util.updateDocument(userId, "totalDefense", shopItem.defence);
				embed.WithTitle("Item bought!");
				embed.WithThumbnailUrl(user.GetAvatarUrl());
				embed.AddInlineField("Attack", userInfo.totalAttack + shopItem.attack);
				embed.AddInlineField("Defense", userInfo.totalDefense + shopItem.defence);
				embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
				await ReplyAsync("", embed: embed);
			}
		}
	}
}

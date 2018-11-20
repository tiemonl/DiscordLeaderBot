﻿using System;
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
			SupportingMethods.SetupMongoCollection("shop");
			Shop shopItem = SupportingMethods.getShopItem(item);
			var embed = new EmbedBuilder();
			Random r = new Random();
			embed.WithTitle(shopItem.Name);
			embed.AddInlineField("Attack", shopItem.Attack);
			embed.AddInlineField("Defence", shopItem.Defence);
			embed.AddInlineField("Cost", shopItem.Cost);
			embed.AddInlineField("Level Requirement", shopItem.LevelRequirement);
			embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
			await ReplyAsync("", embed: embed);
			SupportingMethods.SetupMongoCollection("userData");
		}

		[Command("buy")]
		public async Task buyItemFromShop(int item) {
			var userName = ((SocketGuildUser)Context.Message.Author);
			SupportingMethods.SetupMongoCollection("shop");
			Shop shopItem = SupportingMethods.getShopItem(item);
			SupportingMethods.SetupMongoCollection("userData");
			UserInfo userInfo = SupportingMethods.getUserInformation(userName.ToString());
			var embed = new EmbedBuilder();
			Random r = new Random();
			if (userInfo.points < shopItem.Cost) {
				await ReplyAsync($"User does not have enough points to buy item.");
			} else {
				SupportingMethods.updateDocument(userName.ToString(), "points", shopItem.Cost*-1);
				SupportingMethods.updateDocument(userName.ToString(), "totalAttack", shopItem.Attack);
				SupportingMethods.updateDocument(userName.ToString(), "totalDefense", shopItem.Defence);
				embed.WithTitle("Item bought!");
				embed.WithThumbnailUrl(userName.GetAvatarUrl());
				embed.AddInlineField("Attack", userInfo.totalAttack + shopItem.Attack);
				embed.AddInlineField("Defense", userInfo.totalDefense + shopItem.Defence);
				embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
				await ReplyAsync("", embed: embed);
			}
		}
	}
}

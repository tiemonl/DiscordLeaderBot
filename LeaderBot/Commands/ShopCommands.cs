using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using LeaderBot.Objects;

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
			embed.WithImageUrl("https://i.etsystatic.com/13065784/r/il/2b3688/1058345301/il_570xN.1058345301_cbw9.jpg");
			embed.AddInlineField("Attack", shopItem.Attack);
			embed.AddInlineField("Defence", shopItem.Defence);
			embed.AddInlineField("Cost", shopItem.Cost);
			embed.AddInlineField("Level Requirement", shopItem.LevelRequirement);
			embed.WithColor(new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256)));
			await ReplyAsync("", embed: embed);
		}
	}
}

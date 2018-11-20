using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LeaderBot {
	public class LeaderBot {
		private readonly DiscordSocketClient client;
		public static char CommandPrefix = '-';
		private readonly CommandService commands;

		public LeaderBot() {
			client = new DiscordSocketClient(new DiscordSocketConfig {
				LogLevel = LogSeverity.Info
			});
			commands = new CommandService();
			client.Log += Log;
			client.UserJoined += UserJoined;
			client.MessageReceived += HandleCommandAsync;
			client.ReactionAdded += ReactionAdded;

		}

		public async Task MainAsync() {
			Console.WriteLine("Which bot to run: ");
			string key = Console.ReadLine();
			if (key.Equals("debug")) {
				CommandPrefix = '?';
			}
			string token = GetKey.getKey(key);

			//Util.SetupMongoDatabase();
			//Util.SetupMongoCollection("userData");
			RoleCheck.setUpClient(client);

			await commands.AddModulesAsync(Assembly.GetEntryAssembly());
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
			var user = reaction.User.Value.ToString();

			Util.updateDocument(user, "reactionCount", 1);

			await RoleCheck.reactionCountRoles(channel, reaction, user);
		}



		private Task Log(LogMessage msg) {
			Logger.Log(msg);
			return Task.CompletedTask;
		}

		private string GetUserName(SocketUser socketUser) {
			string userName = "NULL";
			if (socketUser != null) {
				userName = socketUser.ToString();
			}
			return userName;
		}

		public async Task UserJoined(SocketGuildUser user) {
			var userName = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var id = currentGuild.DefaultChannel.Id;
			await Logger.Log(new LogMessage(LogSeverity.Info, $"{GetType().Name}.UserJoined", $"{userName} joined {currentGuild}"));

			await RoleCheck.createUserInDatabase(userName, id);
		}


		public async Task HandleCommandAsync(SocketMessage messageParam) {
			string userName = "";
			string channelName = "";
			string guildName = "";
			int argPos = 0;
			try {
				var msg = messageParam as SocketUserMessage;

				userName = GetUserName(msg.Author);
				channelName = msg.Channel?.Name ?? "NULL";
				var context = new CommandContext(client, msg);
				guildName = context.Guild?.Name ?? "NULL";
				var channelID = msg.Channel.Id;
				await Logger.Log(new LogMessage(LogSeverity.Info, $"{GetType().Name}.HandleCommandAsync", $"HandleCommandAsync G: {guildName} C: {channelName} User: {userName}  Msg: {msg}"));

				
				//if (!msg.Author.IsBot) {
				//	Util.updateDocument(userName, "numberOfMessages", 1);
				//	Util.updateDocument(userName, "experience", msg.Content.Length);
				//	await RoleCheck.messageCountRoles(msg.Author, channelID);
				//	await RoleCheck.dateJoinedRoles(msg.Author, channelID);
				//}
				//if (msg.Author.Id == 181240813492109312 || msg.Author.Id == 195567858133106697) {
				//	if (msg.MentionedUsers.ToList().Count >= 1) {
				//		await RoleCheck.addRole(msg.MentionedUsers.FirstOrDefault() as SocketGuildUser, "???", msg.Channel.Id);
				//	}
				//}

				if (msg == null)
					return;
				else if (msg.HasCharPrefix(CommandPrefix, ref argPos)) {

					var result = await commands.ExecuteAsync(context, argPos);

					if (!result.IsSuccess) // If execution failed, reply with the error message.
					{
						string message = "Command Failed: " + msg;
						await Logger.Log(new LogMessage(LogSeverity.Error, $"{GetType().Name}.HandleCommandAsync", message));
						await context.Channel.SendMessageAsync(message);
					}
				}
			} catch (Exception e) {
				await Logger.Log(new LogMessage(LogSeverity.Error, $"{GetType().Name}.HandleCommandAsync", $"G:{guildName} C:{channelName} U:{userName} Unexpected Exception {e}", e));
			}
		}
	}
}

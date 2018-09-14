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

            SupportingMethods.SetupDatabase("userData");

			await commands.AddModulesAsync(Assembly.GetEntryAssembly());
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
			var user = reaction.User.Value.ToString();

            SupportingMethods.updateDocument(user, "reactionCount", 1);

			UserInfo userInfo = SupportingMethods.getUserInformation(user);
			if (userInfo != null) {
				if (userInfo.ReactionCount >= 250) {
					await addRole(reaction.User.Value as SocketGuildUser, "Overreaction", channel.Id);
				} else if (userInfo.ReactionCount >= 100) {
					await addRole(reaction.User.Value as SocketGuildUser, "Major reaction", channel.Id);
				} else if (userInfo.ReactionCount >= 50) {
					await addRole(reaction.User.Value as SocketGuildUser, "Reactionary", channel.Id);
				} else if (userInfo.ReactionCount >= 25) {
					await addRole(reaction.User.Value as SocketGuildUser, "Reactor", channel.Id);
				}
			} else {
				await createUserInDatabase(reaction.User.Value as SocketUser, channel.Id);
			}
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

			await createUserInDatabase(userName, id);
		}

		private async Task createUserInDatabase(SocketUser userName, ulong id) {
            SupportingMethods.createUserInDatabase(userName);
			await addRole(userName as SocketGuildUser, "Family", id);
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

                SupportingMethods.updateDocument(userName, "numberOfMessages", 1);
                SupportingMethods.updateDocument(userName, "experience", msg.Content.Length);
                if (!msg.Author.IsBot) {
                    await checkMessageCountForRole(msg.Author, channelID);
                }

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

		public async Task checkMessageCountForRole(SocketUser user, ulong channelID) {
			string userName = user.ToString();
			UserInfo userInfo = SupportingMethods.getUserInformation(userName);
			if (userInfo != null) {
				if (userInfo.IsBetaTester) {
					await addRole(user as SocketGuildUser, "Beta Tester", channelID);
				}
				if (userInfo.NumberOfMessages >= 10000) {
					await addRole(user as SocketGuildUser, "I wrote a novel", channelID);
				} else if (userInfo.NumberOfMessages >= 1000) {
					await addRole(user as SocketGuildUser, "I could write a novel", channelID);
				} else if (userInfo.NumberOfMessages >= 100) {
					await addRole(user as SocketGuildUser, "I'm liking this server", channelID);
				} else if (userInfo.NumberOfMessages >= 1) {
					await addRole(user as SocketGuildUser, "Hi and Welcome!", channelID);
				}
			} else {
				await createUserInDatabase(user, channelID);
			}
		}

		public async Task addRole(SocketGuildUser user, string roleName, ulong channelID) {
			var userName = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => SupportingMethods.stringEquals(x.Name, roleName));
			if (!user.Roles.Contains(role)) {
				await Logger.Log(new LogMessage(LogSeverity.Info, $"{GetType().Name}.addRole", $"{userName} has earned {roleName}"));
				await (user as IGuildUser).AddRoleAsync(role);
				var channelName = client.GetChannel(channelID) as IMessageChannel;
				await channelName.SendMessageAsync($"{userName} has earned **{role.Name}**");
			}
		}
	}
}

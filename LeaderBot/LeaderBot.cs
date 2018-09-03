using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LeaderBot
{
	public class LeaderBot
	{
		private readonly DiscordSocketClient client;
		public static char CommandPrefix = '-';
		private readonly CommandService commands;

		public LeaderBot()
		{
			client = new DiscordSocketClient(new DiscordSocketConfig
			{
                LogLevel = LogSeverity.Info
			});
			commands = new CommandService();

			client.Log += Log;
            client.UserJoined += UserJoined;
			client.MessageReceived += HandleCommandAsync;
		}

		public async Task MainAsync()
		{
			string token = GetKey.getKey();
			await commands.AddModulesAsync(Assembly.GetEntryAssembly());
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private Task Log(LogMessage msg)
		{
            Logger.Log(msg);
			return Task.CompletedTask;
		}

		private string GetUserName(SocketUser socketUser)
		{
			string userName = "NULL";
			if (socketUser != null)
			{
				userName = socketUser.ToString();
				SocketGuildUser user = socketUser as SocketGuildUser;
				if (user?.Nickname != null)
				{
					userName += " NickName: " + user.Nickname;
				}
			}
			return userName;
		}

        public async Task UserJoined(SocketGuildUser user){
            var userName = user as SocketUser;
            var currentGuild = user.Guild as SocketGuild;
            var role = currentGuild.Roles.FirstOrDefault(x => x.Name == "Family");
            await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".UserJoined", userName.ToString() + " joined " + currentGuild.ToString()));
            await (user as IGuildUser).AddRoleAsync(role);
        }

		public async Task HandleCommandAsync(SocketMessage messageParam)
		{
			string userName = "";
			string channelName = "";
			string guildName = "";
			int argPos = 0;
			try
			{
				var msg = messageParam as SocketUserMessage;

				if (msg == null)
					return;
				else if (msg.HasCharPrefix(CommandPrefix, ref argPos))
				{
					userName = GetUserName(msg.Author);
					channelName = msg.Channel?.Name ?? "NULL";
					var context = new CommandContext(client, msg);
					guildName = context.Guild?.Name ?? "NULL";
					await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".HandleCommandAsync", $"HandleCommandAsync G: {guildName} C: {channelName} User: {userName}  Msg: {msg}"));

					var result = await commands.ExecuteAsync(context, argPos);

					if (!result.IsSuccess) // If execution failed, reply with the error message.
					{
						string message = "Command Failed: " + result;
						await Logger.Log(new LogMessage(LogSeverity.Error, "HandleCommandAsync", message));
						//await context.Channel.SendMessageAsync(message);
					}
				}
			}
			catch (Exception e)
			{
				await Logger.Log(new LogMessage(LogSeverity.Error, "HandleCommandAsync", $"G:{guildName} C:{channelName} U:{userName} Unexpected Exception", e));
			}
		}
	}
}

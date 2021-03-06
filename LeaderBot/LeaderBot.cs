﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using MongoDB.Bson;
using LeaderBot.LeadingRoles;
using LeaderBot.Utils;
using System.Runtime.InteropServices;

namespace LeaderBot {
	public class LeaderBot {
		private readonly DiscordSocketClient client;
		public string CommandPrefix;
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

		public async Task MainAsync(string token, string prefix, string mongoUrl) {
			Console.WriteLine("token: {0}", token);
            Console.WriteLine("prefix: {0}", prefix);
            CommandPrefix = prefix;
            Console.WriteLine("\nLeaderBot started!");

			DatabaseUtils.SetupMongoDatabase(mongoUrl);
			DatabaseUtils.ChangeCollection("userData");
			RoleUtils.SetUpClient(client);

			await commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			// Block this task until the program is closed.
			await Task.Delay(-1);
		}

		private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction) {
			var user = reaction.User.Value as SocketGuildUser;
			DatabaseUtils.IncrementDocument(user.Id, "reactionCount", 1);
			await RoleUtils.ReactionCountRoles(channel, reaction, user);
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

			await RoleUtils.CreateUserInDatabase(userName, id);
		}


		public async Task HandleCommandAsync(SocketMessage messageParam) {
			string userName = "";
			string channelName = "";
			string guildName = "";
			int argPos = 0;
			try {
				DatabaseUtils.ChangeCollection("userData");
				var msg = messageParam as SocketUserMessage;
				ulong userId = msg.Author.Id;
				userName = GetUserName(msg.Author);
				channelName = msg.Channel?.Name ?? "NULL";
				var context = new CommandContext(client, msg);
				guildName = context.Guild?.Name ?? "NULL";
				var channelID = msg.Channel.Id;
				await Logger.Log(new LogMessage(LogSeverity.Info, $"{GetType().Name}.HandleCommandAsync", $"HandleCommandAsync G: {guildName} C: {channelName} User: {userName}  Msg: {msg}"));

				var guildUsers = await context.Guild.GetUsersAsync();

				if (!msg.Author.IsBot) {
					DatabaseUtils.IncrementDocument(userId, "numberOfMessages", 1);
					DatabaseUtils.IncrementDocument(userId, "experience", msg.Content.Length);
					await RoleUtils.MessageCountRoles(msg.Author, channelID);
					await RoleUtils.DateJoinedRoles(msg.Author, channelID);
					await PointLeader.CheckForNewLeader(guildUsers, channelID);
				}
				if (msg.Author.Id == 181240813492109312 || msg.Author.Id == 195567858133106697) {
					if (msg.MentionedUsers.ToList().Count >= 1) {
						await RoleUtils.GiveRoleToUser(msg.MentionedUsers.FirstOrDefault() as SocketGuildUser, "???", msg.Channel.Id);
					}
				}

				if (msg == null)
					return;
				else if (msg.HasStringPrefix(CommandPrefix, ref argPos)) {
					var result = await commands.ExecuteAsync(context, argPos, null);

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

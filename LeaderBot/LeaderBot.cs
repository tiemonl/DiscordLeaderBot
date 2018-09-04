using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LeaderBot
{
	public class LeaderBot
	{
		private readonly DiscordSocketClient client;
		public static char CommandPrefix = '-';
		private readonly CommandService commands;
		private IMongoCollection<BsonDocument> userInfoCollection;
		MongoClient mongoClient;
		IMongoDatabase db;

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
			var connectionString = "mongodb://localhost:27017";

			mongoClient = new MongoClient(connectionString);
			db = mongoClient.GetDatabase("Leaderbot");
			userInfoCollection = db.GetCollection<BsonDocument>("userData");

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
			await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".UserJoined", userName.ToString() + " joined " + currentGuild.ToString()));
			await addRole(user, "Family", 471383490185658400);
			var document = new BsonDocument
			{
				{ "name", userName.ToString() },
                { "dateJoined", DateTime.Now.ToString() },
				{ "numberOfMessages", 0 },
				{ "isBetaTester", false }
			};

			await userInfoCollection.InsertOneAsync(document);
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

				userName = GetUserName(msg.Author);
				channelName = msg.Channel?.Name ?? "NULL";
				var context = new CommandContext(client, msg);
				guildName = context.Guild?.Name ?? "NULL";
				var channelID = msg.Channel.Id;
				await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".HandleCommandAsync", $"HandleCommandAsync G: {guildName} C: {channelName} User: {userName}  Msg: {msg}"));

				var filterUserName = Builders<BsonDocument>.Filter.Eq("name", userName.ToString());
				var update = new BsonDocument("$inc", new BsonDocument { { "numberOfMessages", 1 } });
				await userInfoCollection.FindOneAndUpdateAsync(filterUserName, update);

				await checkMessageCountForRole(userName, msg.Author, channelID);

				if (msg == null)
					return;
				else if (msg.HasCharPrefix(CommandPrefix, ref argPos))
				{

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
				await Logger.Log(new LogMessage(LogSeverity.Error, "HandleCommandAsync", $"G:{guildName} C:{channelName} U:{userName} Unexpected Exception {e}", e));
			}
		}

		public async Task checkMessageCountForRole(string userName, SocketUser user, ulong channelID) {
			var filterUserName = Builders<BsonDocument>.Filter.Eq("name", userName.ToString());
			var doc = userInfoCollection.Find(filterUserName).FirstOrDefault();
			if (doc != null) {
				string jsonText = "{"+doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
				Console.WriteLine(jsonText);
				var userInformation = JsonConvert.DeserializeObject<UserInfo>(jsonText);
                Console.WriteLine($"Hello userId : {userInformation.Name} and number of messages : {userInformation.NumberOfMessages} joined: {userInformation.DateJoined} and is a beta tester: {userInformation.IsBetaTester}");
                if (userInformation.IsBetaTester) {
					await addRole(user as SocketGuildUser, "Beta Tester", channelID);
				}
				if (userInformation.NumberOfMessages >= 10000) {
					await addRole(user as SocketGuildUser, "I wrote a novel", channelID);
				} else if(userInformation.NumberOfMessages >= 1000) {
					await addRole(user as SocketGuildUser, "I could write a novel", channelID);
				} else if (userInformation.NumberOfMessages >= 100) {
					await addRole(user as SocketGuildUser, "I'm liking this server", channelID);
				} else if (userInformation.NumberOfMessages >= 1) {
					await addRole(user as SocketGuildUser, "Hi and Welcome!", channelID);
				}
				
			} else {
				Console.WriteLine("No user found.");
			}
		}

		public async Task addRole(SocketGuildUser user, string roleName, ulong channelID) {
			var userName = user as SocketUser;
			var currentGuild = user.Guild as SocketGuild;
			var role = currentGuild.Roles.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower());
			if (!user.Roles.Contains(role)) {
				await Logger.Log(new LogMessage(LogSeverity.Info, GetType().Name + ".addRole", userName.ToString() + " has earned " + roleName));
				await (user as IGuildUser).AddRoleAsync(role);
				var channelName = client.GetChannel(channelID) as IMessageChannel;
				await channelName.SendMessageAsync($"{userName} has earned **{role.Name}**");
			}
		}
	}
}

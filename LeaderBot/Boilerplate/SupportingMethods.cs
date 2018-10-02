using System;
using Discord;
using Discord.WebSocket;
using LeaderBot.Points;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace LeaderBot {
	/// <summary>
	/// This is used to alleviate boilerplate code
	/// </summary>
	public class SupportingMethods {
		private static MongoClient Client;
		private static IMongoDatabase Database;
		private static IMongoCollection<BsonDocument> Collection;

		/// <summary>
		/// Strings the equals.
		/// </summary>
		/// <returns><c>true</c>, if equals was strung, <c>false</c> otherwise.</returns>
		/// <param name="a">The string to match</param>
		/// <param name="b">The string to compare</param>
		public static bool stringEquals(string a, string b) {
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the mongo collection and sets up the connection.
		/// I am only passing in the collection name, because I am using only one database currently.
		/// </summary>
		/// <returns>The mongo collection</returns>
		/// <param name="collectionName">Collection name in the MongoDB</param>
		public static void SetupMongoDatabase() {
			string connectionString = null;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				connectionString = "mongodb://localhost:27017";
			else {
				connectionString = Resources.mongoconnection;
			}
			Client = new MongoClient(connectionString);
			Database = Client.GetDatabase("Leaderbot");

		}

		public static void SetupMongoCollection(string collectionName) {
			Collection = Database.GetCollection<BsonDocument>(collectionName);
		}

		public static BsonDocument findBsonDocumentByFieldCriteria(string field, string criteria) {
			var result = Collection.Find(filterDocumentByFieldCriteria(field, criteria)).FirstOrDefault();
			return result;
		}

		public static FilterDefinition<BsonDocument> filterDocumentByFieldCriteria(string field, string criteria) {
			var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
			return filter;
		}

		public static void updateArray(string filterField, string filterCriteria, string arrayField, string arrayCriteria) {
			var filter = filterDocumentByFieldCriteria(filterField, filterCriteria);

			var update = Builders<BsonDocument>.Update.Push(arrayField, arrayCriteria);

			Collection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document.
		/// </summary>
		/// <param name="user">User to match to</param>
		/// <param name="field">Field to update</param>
		/// <param name="updateCount">the amount to increase the field by. Default is zero</param>
		public static void updateDocument(string user, string field, int updateCount = 0) {
			var filterUserName = filterDocumentByFieldCriteria("name", user);
			var update = new BsonDocument("$inc", new BsonDocument { { field, updateCount } });
			Collection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Gets the user information from the database and stores in the UserInformation Object
		/// </summary>
		/// <returns>The user information</returns>
		/// <param name="user">User to get the information from</param>
		public static UserInfo getUserInformation(string user) {
			UserInfo userInformation = null;
			var doc = findBsonDocumentByFieldCriteria("name", user);
			if (doc != null) {
				string jsonText = "{" + doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
				userInformation = JsonConvert.DeserializeObject<UserInfo>(jsonText);

			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(SupportingMethods).Name}.getUserInformation", "Could not find user!"));
			}
			return userInformation;
		}

		public static PointsReceived getPointsReceived(string field, string criteria) {
			PointsReceived userInformation = null;
			var doc = findBsonDocumentByFieldCriteria(field, criteria);
			if (doc == null) {
				createNewDatePointsReceived(criteria);
				doc = findBsonDocumentByFieldCriteria(field, criteria);
			}
			if (doc != null) {
				string jsonText = "{" + doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
				userInformation = JsonConvert.DeserializeObject<PointsReceived>(jsonText);

			} else {

				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(SupportingMethods).Name}.getUserInformation", "Could not find user!"));
			}
			return userInformation;
		}

		public static void createNewDatePointsReceived(string date) {
			var document = new BsonDocument {
				{ "date", date },
				{ "users",  new BsonArray{ } }
				};
			Collection.InsertOneAsync(document);
		}

		public static List<Roles> LoadAllRolesFromServer() {
			SetupMongoCollection("roles");
			List<Roles> allRolesInServer = new List<Roles>();

			using (var cursor = Collection.Find(new BsonDocument()).ToCursor()) {
				while (cursor.MoveNext()) {
					foreach (var doc in cursor.Current) {
						string jsonText = "{" + doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
						allRolesInServer.Add(JsonConvert.DeserializeObject<Roles>(jsonText));
					}
				}
			}
			SetupMongoCollection("userData");
			return allRolesInServer;
		}

		public static void createUserInDatabase(SocketUser userName) {
			var user = userName as SocketGuildUser;
			var date = user.JoinedAt.ToString();

			var document = new BsonDocument
			{
				{ "name", userName.ToString() },
				{ "dateJoined",  date},
				{ "numberOfMessages", 0 },
				{ "isBetaTester", false },
				{ "reactionCount",  0 },
				{ "experience", 0 },
				{ "points", 0 },
				{ "winCoinflipStreak", 0 },
				{ "loseCoinflipStreak", 0 },
				{ "roles",  new BsonArray{ } }
			};
			Collection.InsertOneAsync(document);
		}

		//edit as needed
		public static void updateDocumentField(string user, string field, int value){
			var doc = findBsonDocumentByFieldCriteria("name", user);
			if (!doc.Contains(field)){
				var update = Builders<BsonDocument>.Update.Set(field, value);
				Collection.UpdateOneAsync(filterDocumentByFieldCriteria("name", user), update);
			}
		}


		public static void createEquipmentInShop(string name, int atk, int def, int cost, int levelReq) {
			SetupMongoCollection("shop");
			var document = new BsonDocument
			{
				{ "_id", Collection.CountDocuments(new BsonDocument())+1 },
				{ "name",  name},
				{ "ATK", atk },
				{ "DEF", def },
				{ "cost",  cost },
				{ "levelRequirement", levelReq }
			};
			Collection.InsertOneAsync(document);
			SetupMongoCollection("userData");
		}

		public static void createRoleInDatabase(string name, string description, int difficulty) {
			SetupMongoCollection("roles");
			var document = new BsonDocument
			{
				{ "name", name },
				{ "description",  description},
				{ "difficulty", difficulty }
			};
			Collection.InsertOneAsync(document);
			SetupMongoCollection("userData");
		}

		public static StringBuilder createLeaderboard(string leaderboardName, IReadOnlyCollection<IGuildUser> guildUsers, int userCount) {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("```css\n");
			string titleString = "Username".PadRight(30) + "| Total " + leaderboardName;
			stringBuilder.Append(titleString);
			stringBuilder.Append("\n".PadRight(titleString.Length + 1, '-') + "\n");
			var allUsers = new Dictionary<SocketGuildUser, int>();

			foreach (var user in guildUsers) {
				if (!user.IsBot) {
					var userName = user as SocketUser;
					UserInfo userInfo = getUserInformation(userName.ToString());
					if (userInfo != null) {
						if (leaderboardName.Equals("Experience")) {
							allUsers.Add(user as SocketGuildUser, userInfo.Experience);
						} else if (leaderboardName.Equals("Roles")) {
							allUsers.Add(user as SocketGuildUser, userInfo.Roles.Length - 1); //-1: remove @everyone
						} else if (leaderboardName.Equals("Points")) {
							allUsers.Add(user as SocketGuildUser, userInfo.Points);
						} else if (leaderboardName.Equals("Messages")) {
							allUsers.Add(user as SocketGuildUser, userInfo.NumberOfMessages);
						} else if (leaderboardName.Equals("Reactions")) {
							allUsers.Add(user as SocketGuildUser, userInfo.ReactionCount);
						}
					}
				}
			}
			if (allUsers.Count > 0) {
				var sortedDict = from entry in allUsers orderby entry.Value descending select entry;
				int i = 0;
				foreach (var entry in sortedDict) {
					stringBuilder.Append(entry.Key.ToString().PadRight(30) + "|\t" + entry.Value + "\n");
					++i;
					if (i >= userCount) {
						break;
					}
				}
				stringBuilder.Append("```");
			}
			return stringBuilder;
		}
	}
}

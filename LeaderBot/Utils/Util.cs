using System;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LeaderBot.Objects;
using MongoDB.Bson.Serialization;
using LeaderBot.Competition;

namespace LeaderBot.Utils {
	/// <summary>
	/// This is used to alleviate boilerplate code
	/// </summary>
	public class Util {

		/// <summary>
		/// Strings the equals.
		/// </summary>
		/// <returns><c>true</c>, if equals was strung, <c>false</c> otherwise.</returns>
		/// <param name="a">The string to match</param>
		/// <param name="b">The string to compaSe</param>
		public static bool StringEquals(string a, string b) {
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}


		public static void UpdateArray(string filterField, string filterCriteria, string arrayField, string arrayCriteria, string collectionName = "userData") {
			var filter = DatabaseUtils.FilterMongoDocument(filterField, filterCriteria, collectionName);

			var update = Builders<BsonDocument>.Update.Push(arrayField, arrayCriteria);

			DatabaseUtils.MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Gets the user information from the database and stores in the UserInformation Object
		/// </summary>
		/// <returns>The user information</returns>
		/// <param name="user">User to get the information from</param>
		public static UserInfo GetUserInformation(ulong user) {
			UserInfo userInformation = null;
			var doc = DatabaseUtils.FindMongoDocument("_id", user, "userData");
			if (doc != null) {
				userInformation = BsonSerializer.Deserialize<UserInfo>(doc);
			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(Util).Name}.getUserInformation", $"Could not find user {user}!"));
			}
			return userInformation;
		}

		public static Shop GetShopItem(int item) {
			Shop shopItem = null;
			var doc = DatabaseUtils.FindMongoDocument("_id", item, "shop");
			if (doc != null) {
				shopItem = BsonSerializer.Deserialize<Shop>(doc);
			}
			return shopItem;
		}

        public static PointBank GetPointBank(string bank) {
            PointBank pointBank = null;
            var doc = DatabaseUtils.FindMongoDocument("_id", bank, "pointBanks");
            if (doc != null) {
                pointBank = BsonSerializer.Deserialize<PointBank>(doc);
            }
            return pointBank;
        }

        public static DailyPoints GetDailyPoints(string field, string criteria) {
            DailyPoints dailyPoints = null;
			var doc = DatabaseUtils.FindMongoDocument(field, criteria, "dailyPoints");
			if (doc == null) {
				CreateNewDailyPoints(criteria);
				doc = DatabaseUtils.FindMongoDocument(field, criteria, "dailyPoints");
			}
			if (doc != null) {
                dailyPoints = BsonSerializer.Deserialize<DailyPoints>(doc);
			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(Util).Name}.GetDailyPoints", "Could not find date!"));
			}
			return dailyPoints;
		}

		public static void CreateNewDailyPoints(string date) {
			var document = new BsonDocument {
				{ "_id", date},
				{ "users",  new BsonArray{ } }
				};
			DatabaseUtils.MyMongoCollection.InsertOneAsync(document);
		}

		public static List<Roles> LoadAllRolesFromServer() {
			DatabaseUtils.ChangeCollection("roles");
			List<Roles> allRolesInServer = new List<Roles>();

			using (var cursor = DatabaseUtils.MyMongoCollection.Find(new BsonDocument()).ToCursor()) {
				while (cursor.MoveNext()) {
					foreach (var doc in cursor.Current) {
						string jsonText = "{" + doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
						allRolesInServer.Add(JsonConvert.DeserializeObject<Roles>(jsonText));
					}
				}
			}
			DatabaseUtils.ChangeCollection("userData");
			return allRolesInServer;
		}

		public static void CreateUserInDatabase(SocketUser userName) {
			var user = userName as SocketGuildUser;
			var date = user.JoinedAt.ToString();
			var document = new BsonDocument
			{
				{"_id", (long)user.Id},
				{ "name", userName.ToString() },
				{ "dateJoined",  date},
				{ "numberOfMessages", 0 },
				{ "isBetaTester", false },
				{ "reactionCount",  0 },
				{ "experience", 0 },
				{ "points", 0 },
				{ "winCoinflipStreak", 0 },
				{ "loseCoinflipStreak", 0 },
				{ "roles",  new BsonArray{ } },
				{ "totalAttack", 0 },
				{ "totalDefense", 0 }
			};
			DatabaseUtils.ChangeCollection("userData");
			DatabaseUtils.MyMongoCollection.InsertOneAsync(document);
		}

		public static void CreateUserInCompetition(SocketUser userName) {
			var user = userName as SocketGuildUser;
			var document = new BsonDocument
			{
				{"_id", (long)user.Id},
				{ "name", userName.ToString() },
				{ "credits", 10000 }
			};
			DatabaseUtils.ChangeCollection("competition");
			DatabaseUtils.MyMongoCollection.InsertOneAsync(document);
		}
		public static UsersEntered GetUsersInCompetition(string field, ulong criteria) {
			UsersEntered usersEntered = null;
			var doc = DatabaseUtils.FindMongoDocument(field, criteria, "competition");
			if (doc == null) {
				return null;
			}
			if (doc != null) {
				usersEntered = BsonSerializer.Deserialize<UsersEntered>(doc);
			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(Util).Name}.getUsersInCompetition", "Could not find date!"));
			}
			return usersEntered;
		}

		//edit as needed
		//FIXME: needs to be moved and worked on
		public static void UpdateDocumentField(ulong user, string field, BsonArray value) {
			var doc = DatabaseUtils.FindMongoDocument("_id", user);
			if (doc != null) {
				var update = Builders<BsonDocument>.Update.Set(field, value);
				DatabaseUtils.MyMongoCollection.UpdateOneAsync(DatabaseUtils.FilterMongoDocument("_id", user), update);
			}
		}


		public static void CreateEquipmentInShop(string name, int atk, int def, int cost, int levelReq) {
			DatabaseUtils.ChangeCollection("shop");
			var document = new BsonDocument
			{
				{ "_id", DatabaseUtils.MyMongoCollection.CountDocuments(new BsonDocument())+1 },
				{ "name",  name},
				{ "attack", atk },
				{ "defence", def },
				{ "cost",  cost },
				{ "levelRequirement", levelReq }
			};
			DatabaseUtils.MyMongoCollection.InsertOneAsync(document);
			DatabaseUtils.ChangeCollection("userData");
		}

		public static void CreateRoleInDatabase(string name, string description, int difficulty) {
			DatabaseUtils.ChangeCollection("roles");
			var document = new BsonDocument
			{
				{ "name", name },
				{ "description",  description},
				{ "difficulty", difficulty }
			};
			DatabaseUtils.MyMongoCollection.InsertOneAsync(document);
			DatabaseUtils.ChangeCollection("userData");
		}

		public static StringBuilder CreateLeaderboard(string leaderboardName, IReadOnlyCollection<IGuildUser> guildUsers, int userCount) {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("```css\n");
			string titleString = "Username".PadRight(30) + "| Total " + leaderboardName;
			stringBuilder.Append(titleString);
			stringBuilder.Append("\n".PadRight(titleString.Length + 1, '-') + "\n");
			var allUsers = new Dictionary<SocketGuildUser, int>();

			foreach (var user in guildUsers) {
				if (!user.IsBot) {
					UserInfo userInfo = GetUserInformation(user.Id);
					if (userInfo != null) {
						if (leaderboardName.Equals("Experience")) {
							allUsers.Add(user as SocketGuildUser, userInfo.experience);
						} else if (leaderboardName.Equals("Roles")) {
							allUsers.Add(user as SocketGuildUser, userInfo.roles.Length - 1); //-1: remove @everyone
						} else if (leaderboardName.Equals("Points")) {
							allUsers.Add(user as SocketGuildUser, userInfo.points);
						} else if (leaderboardName.Equals("Messages")) {
							allUsers.Add(user as SocketGuildUser, userInfo.numberOfMessages);
						} else if (leaderboardName.Equals("Reactions")) {
							allUsers.Add(user as SocketGuildUser, userInfo.reactionCount);
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

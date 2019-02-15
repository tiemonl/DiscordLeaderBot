using System;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace LeaderBot.Utils {
	/// <summary>
	/// This is used to alleviate boilerplate code
	/// </summary>
	public static class Util {

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
            var doc = DatabaseUtils.FindMongoDocument(filterField, filterCriteria);
            if (doc != null) {
                var docArray = doc.FirstOrDefault(x => x.Name == arrayField).Value.AsBsonArray;
                if (docArray.Contains(arrayCriteria)) {
                    Logger.Log(new LogMessage(LogSeverity.Warning, MethodBase.GetCurrentMethod().DeclaringType.FullName + ".UpdateArray", $"Array already contains {arrayCriteria}"));
                    return;
                }
                var filter = DatabaseUtils.FilterMongoDocument(filterField, filterCriteria, collectionName);
                var update = Builders<BsonDocument>.Update.Push(arrayField, arrayCriteria);
                DatabaseUtils.MyMongoCollection.FindOneAndUpdateAsync(filter, update);
            }
        }

        public static void UpdateArray(string filterField, ulong filterCriteria, string arrayField, string arrayCriteria, string collectionName = "userData") {
            var doc = DatabaseUtils.FindMongoDocument(filterField, filterCriteria);
            if (doc != null) {
                var docArray = doc.FirstOrDefault(x => x.Name == arrayField).Value.AsBsonArray;
                if (docArray.Contains(arrayCriteria)) {
                    Logger.Log(new LogMessage(LogSeverity.Warning, MethodBase.GetCurrentMethod().DeclaringType.FullName + ".UpdateArray", $"Array already contains {arrayCriteria}"));
                    return;
                }
                var filter = DatabaseUtils.FilterMongoDocument(filterField, filterCriteria, collectionName);
                var update = Builders<BsonDocument>.Update.Push(arrayField, arrayCriteria);
                DatabaseUtils.MyMongoCollection.FindOneAndUpdateAsync(filter, update);
            }
        }

        public static void UpdateArray(string filterField, string filterCriteria, string arrayField, ulong arrayCriteria, string collectionName = "userData") {
            var doc = DatabaseUtils.FindMongoDocument(filterField, filterCriteria, collectionName);
            if (doc != null) {
                var docArray = doc.FirstOrDefault(x => x.Name == arrayField).Value.AsBsonArray;
                if (docArray.Contains(arrayCriteria.ToString())) {
                    Logger.Log(new LogMessage(LogSeverity.Warning, MethodBase.GetCurrentMethod().DeclaringType.FullName + ".UpdateArray", $"Array already contains {arrayCriteria}"));
                    return;
                }
                var filter = DatabaseUtils.FilterMongoDocument(filterField, filterCriteria, collectionName);
                var update = Builders<BsonDocument>.Update.Push(arrayField, arrayCriteria);
                DatabaseUtils.MyMongoCollection.FindOneAndUpdateAsync(filter, update);
            }
        }

        public static void UpdateRemoveArray(string filterField, string filterCriteria, string arrayField, string arrayCriteria, string collectionName = "userData") {
            var doc = DatabaseUtils.FindMongoDocument(filterField, filterCriteria);
            if (doc != null) {
                var docArray = doc.FirstOrDefault(x => x.Name == arrayField).Value.AsBsonArray;
                if (!docArray.Contains(arrayCriteria)) {
                    Logger.Log(new LogMessage(LogSeverity.Error, MethodBase.GetCurrentMethod().DeclaringType.FullName + ".UpdateRemoveArray", $"Array does not contain {arrayCriteria}"));
                    return;
                }
                var filter = DatabaseUtils.FilterMongoDocument(filterField, filterCriteria, collectionName);
                var update = Builders<BsonDocument>.Update.Pull(arrayField, arrayCriteria);
                DatabaseUtils.MyMongoCollection.FindOneAndUpdateAsync(filter, update);
            }
        }

        public static List<Roles> LoadAllRolesFromServer() {
			DatabaseUtils.ChangeCollection("roles");
			List<Roles> allRolesInServer = new List<Roles>();

			using (var cursor = DatabaseUtils.MyMongoCollection.Find(new BsonDocument()).ToCursor()) {
				while (cursor.MoveNext()) {
					foreach (var doc in cursor.Current) {
						allRolesInServer.Add(BsonSerializer.Deserialize<Roles>(doc));
					}
				}
			}
			DatabaseUtils.ChangeCollection("userData");
			return allRolesInServer;
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
		
		public static StringBuilder CreateLeaderboard(string leaderboardName, IReadOnlyCollection<IGuildUser> guildUsers, int userCount) {
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("```css\n");
			string titleString = "Username".PadRight(30) + "| Total " + leaderboardName;
			stringBuilder.Append(titleString);
			stringBuilder.Append("\n".PadRight(titleString.Length + 1, '-') + "\n");
			var allUsers = new Dictionary<SocketGuildUser, int>();

			foreach (var user in guildUsers) {
				if (!user.IsBot) {
					UserInfo userInfo = ObjectUtils.GetUserInformation(user.Id);
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

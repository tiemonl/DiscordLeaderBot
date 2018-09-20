using System;
using Discord;
using Discord.WebSocket;
using LeaderBot.Points;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

		/// <summary>
		/// Filters the name of the document by user
		/// </summary>
		/// <returns>The document by user name</returns>
		/// <param name="user">User to match to</param>
		public static FilterDefinition<BsonDocument> filterDocumentByUserName(string user) {
			var filter = Builders<BsonDocument>.Filter.Eq("name", user);
			return filter;
		}

		/// <summary>
		/// Finds the name of the bson document by user
		/// </summary>
		/// <returns>The bson document by user name</returns>
		/// <param name="user">User to find in the collection</param>
		public static BsonDocument findBsonDocumentByUserName(string user) {
			var result = Collection.Find(filterDocumentByUserName(user)).FirstOrDefault();
			return result;
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
			var filter = Builders<BsonDocument>.Filter.Eq(filterField, filterCriteria);

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
			var filterUserName = filterDocumentByUserName(user);
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
			var doc = findBsonDocumentByUserName(user);
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
			//var jsons = Collection.Find(_ => true);
			//string json = jsons.ToJson();
			//json = "{" + json.Substring(json.IndexOf(',') + 1);
			//allRolesInServer = JsonConvert.DeserializeObject<List<Roles>>(json);
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
				{ "points", 0 }
			};
			Collection.InsertOneAsync(document);
		}
	}
}

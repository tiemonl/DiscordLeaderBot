using System;
using Discord;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace LeaderBot {
	/// <summary>
	/// This is used to alleviate boilerplate code
	/// </summary>
	class SupportingMethods {
		MongoClient Client;
		IMongoDatabase Database;
		IMongoCollection<BsonDocument> Collection;

		/// <summary>
		/// Strings the equals.
		/// </summary>
		/// <returns><c>true</c>, if equals was strung, <c>false</c> otherwise.</returns>
		/// <param name="a">The string to match</param>
		/// <param name="b">The string to compare</param>
		public bool stringEquals(string a, string b) {
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Gets the mongo collection and sets up the connection.
		/// I am only passing in the collection name, because I am using only one database currently.
		/// </summary>
		/// <returns>The mongo collection</returns>
		/// <param name="collectionName">Collection name in the MongoDB</param>
		public void SetupDatabase(string collectionName) {
			string connectionString = "mongodb://192.168.0.123:27017";

			Client = new MongoClient(connectionString);
			Database = Client.GetDatabase("Leaderbot");
			Collection = Database.GetCollection<BsonDocument>(collectionName);
		}

		/// <summary>
		/// Filters the name of the document by user
		/// </summary>
		/// <returns>The document by user name</returns>
		/// <param name="user">User to match to</param>
		public FilterDefinition<BsonDocument> filterDocumentByUserName(string user) {
			return Builders<BsonDocument>.Filter.Eq("name", user);
		}

		/// <summary>
		/// Finds the name of the bson document by user
		/// </summary>
		/// <returns>The bson document by user name</returns>
		/// <param name="user">User to find in the collection</param>
		public BsonDocument findBsonDocumentByUserName(string user) {
			return Collection.Find(filterDocumentByUserName(user)).FirstOrDefault();
		}

		/// <summary>
		/// Updates the document.
		/// </summary>
		/// <param name="user">User to match to</param>
		/// <param name="field">Field to update</param>
		/// <param name="updateCount">the amount to increase the field by. Default is zero</param>
		public void updateDocument(string user, string field, int updateCount = 0) {
			var filterUserName = filterDocumentByUserName(user);
			var update = new BsonDocument("$inc", new BsonDocument { { field, updateCount } });
			Collection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Gets the user information from the database and stores in the UserInformation Object
		/// </summary>
		/// <returns>The user information</returns>
		/// <param name="user">User to get the information from</param>
		public UserInfo getUserInformation(string user) {
			UserInfo userInformation = null;
			var doc = findBsonDocumentByUserName(user);
			if (doc != null) {
				string jsonText = "{" + doc.ToJson().Substring(doc.ToJson().IndexOf(',') + 1);
				userInformation = JsonConvert.DeserializeObject<UserInfo>(jsonText);

			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{GetType().Name}.getUserInformation", "Could not find user!"));
			}
			return userInformation;
		}
	}
}

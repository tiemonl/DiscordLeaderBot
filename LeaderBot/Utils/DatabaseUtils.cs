using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.InteropServices;

namespace LeaderBot.Utils {
	public static class DatabaseUtils {
        public static IMongoDatabase MyMongoDatabase { get; private set; }
        public static IMongoCollection<BsonDocument> MyMongoCollection { get; private set; }
        public static MongoClient MyMongoClient { get; private set; }

        /// <summary>
        /// Sets up the mongo connection to the server
        /// </summary>
        public static void SetupMongoDatabase() {
            var connectionString = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 
                Resources.mongoconnectionserver : Resources.mongoconnection;
            MyMongoClient = new MongoClient(connectionString);
            MyMongoDatabase = MyMongoClient.GetDatabase("Leaderbot");

        }

        /// <summary>
        /// Used to change between collections during runtime.
        /// </summary>
        /// <param name="collectionName">Name of the collection to switch to</param>
        public static void ChangeCollection(string collectionName) {
            MyMongoCollection = MyMongoDatabase.GetCollection<BsonDocument>(collectionName);
        }

		#region FindMongoDocument overloads
		/// <summary>
		/// Finds the mongo document specified.
		/// </summary>
		/// <returns>The found mongo document. Returns null if not found. </returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field. ie: "name" : "Liam"</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static BsonDocument FindMongoDocument(string field, string criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria, collectionName)).FirstOrDefault();
            return result;
        }

		/// <summary>
		/// Finds the mongo document specified.
		/// </summary>
		/// <returns>The found mongo document. Returns null if not found. </returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field. ie. "_id" : "181240813492109312"</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static BsonDocument FindMongoDocument(string field, ulong criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
            var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria, collectionName)).FirstOrDefault();
            return result;
        }

		/// <summary>
		/// Finds the mongo document specified.
		/// </summary>
		/// <returns>The found mongo document. Returns null if not found. </returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field. ie. "points" : "1400"</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static BsonDocument FindMongoDocument(string field, int criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria, collectionName)).FirstOrDefault();
            return result;
        }

		/// <summary>
		/// Finds the mongo document specified.
		/// </summary>
		/// <returns>The found mongo document. Returns null if not found. </returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field. ie. "isBeta" : "true"</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static BsonDocument FindMongoDocument(string field, bool criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria, collectionName)).FirstOrDefault();
            return result;
        }
		#endregion

		#region FilterMongoDocument overloads
		/// <summary>
		/// Filters the specified mongo document.
		/// </summary>
		/// <returns>The mongo document.</returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, string criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
			return filter;
        }

		/// <summary>
		/// Filters the specified mongo document.
		/// </summary>
		/// <returns>The mongo document.</returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, ulong criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
			return filter;
        }

		/// <summary>
		/// Filters the specified mongo document.
		/// </summary>
		/// <returns>The mongo document.</returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, int criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
			return filter;
        }

		/// <summary>
		/// Filters the specified mongo document.
		/// </summary>
		/// <returns>The mongo document.</returns>
		/// <param name="field">Field to match to.</param>
		/// <param name="criteria">Criteria for the specified field.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, bool criteria, string collectionName = "userData") {
			ChangeCollection(collectionName);
			var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
			return filter;
        }
		#endregion

		#region Increment/Decrement document overloads
		/// <summary>
		/// Increments the specifed document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="incrementAmount">Increment amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void IncrementDocument(ulong idValue, string field, int incrementAmount = 0, string collectionName = "userData") {
            var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$inc", new BsonDocument { { field, incrementAmount } });
            MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
        }

		/// <summary>
		/// Increments the specifed document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="incrementAmount">Increment amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void IncrementDocument(string idValue, string field, int incrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, incrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Increments the specifed document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="incrementAmount">Increment amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void IncrementDocument(int idValue, string field, int incrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, incrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Increments the specifed document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="incrementAmount">Increment amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void IncrementDocument(bool idValue, string field, int incrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, incrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/*seperator of inc/dec*/

		/// <summary>
		/// Decrements the specified document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="decrementAmount">Decrement amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void DecrementDocument(ulong idValue, string field, int decrementAmount = 0, string collectionName = "userData") {
            var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$inc", new BsonDocument { { field, -decrementAmount } });
            MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
        }

		/// <summary>
		/// Decrements the specified document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="decrementAmount">Decrement amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void DecrementDocument(string idValue, string field, int decrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, -decrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Decrements the specified document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="decrementAmount">Decrement amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void DecrementDocument(int idValue, string field, int decrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, -decrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}

		/// <summary>
		/// Decrements the specified document field for the specified idValue by the given amount.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="decrementAmount">Decrement amount.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void DecrementDocument(bool idValue, string field, int decrementAmount = 0, string collectionName = "userData") {
			var filterUserName = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$inc", new BsonDocument { { field, -decrementAmount } });
			MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
		}
		#endregion

		#region UpdateDocumentValue overloads
		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(ulong idValue, string field, string value, string collectionName = "userData") {
            var filter = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(ulong idValue, string field, int value, string collectionName = "userData") {
            var filter = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(ulong idValue, string field, long value, string collectionName = "userData") {
            var filter = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(ulong idValue, string field, bool value, string collectionName = "userData") {
            var filter = FilterMongoDocument("_id", idValue, collectionName);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(ulong idValue, string field, ulong value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, (long)value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(string idValue, string field, ulong value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, (long)value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(string idValue, string field, string value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(string idValue, string field, int value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(string idValue, string field, bool value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}

		/// <summary>
		/// Updates the document value for the given field.
		/// </summary>
		/// <param name="idValue">idValue's document to find.</param>
		/// <param name="field">Field to update in the document.</param>
		/// <param name="value">Value to change.</param>
		/// <param name="collectionName">Collection to edit, default value of "userData"</param>
		public static void UpdateDocumentValue(string idValue, string field, long value, string collectionName = "userData") {
			var filter = FilterMongoDocument("_id", idValue, collectionName);
			var update = new BsonDocument("$set", new BsonDocument { { field, value } });
			MyMongoCollection.FindOneAndUpdateAsync(filter, update);
		}
		#endregion
	}
}

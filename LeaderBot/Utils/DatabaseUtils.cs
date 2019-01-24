using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Driver;

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
        public static BsonDocument FindMongoDocument(string field, string criteria) {
            var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Finds the mongo document specified.
        /// </summary>
        /// <returns>The found mongo document. Returns null if not found. </returns>
        /// <param name="field">Field to match to.</param>
        /// <param name="criteria">Criteria for the specified field. ie. "_id" : "181240813492109312"</param>
        public static BsonDocument FindMongoDocument(string field, ulong criteria) {
            var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Finds the mongo document specified.
        /// </summary>
        /// <returns>The found mongo document. Returns null if not found. </returns>
        /// <param name="field">Field to match to.</param>
        /// <param name="criteria">Criteria for the specified field. ie. "points" : "1400"</param>
        public static BsonDocument FindMongoDocument(string field, int criteria) {
            var result = MyMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
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
        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, string criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }

        /// <summary>
        /// Filters the specified mongo document.
        /// </summary>
        /// <returns>The mongo document.</returns>
        /// <param name="field">Field to match to.</param>
        /// <param name="criteria">Criteria for the specified field.</param>
        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, ulong criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }

        /// <summary>
        /// Filters the specified mongo document.
        /// </summary>
        /// <returns>The mongo document.</returns>
        /// <param name="field">Field to match to.</param>
        /// <param name="criteria">Criteria for the specified field.</param>
        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, int criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }
        #endregion

        /// <summary>
        /// Increments the specifed document field for the specified user by the given amount.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="incrementAmount">Increment amount.</param>
        public static void IncrementDocument(ulong user, string field, int incrementAmount = 0) {
            var filterUserName = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$inc", new BsonDocument { { field, incrementAmount } });
            MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
        }

        /// <summary>
        /// Decrements the specified document field for the specified user by the given amount.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="decrementAmount">Decrement amount.</param>
        public static void DecrementDocument(ulong user, string field, int decrementAmount = 0) {
            var filterUserName = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$inc", new BsonDocument { { field, -decrementAmount } });
            MyMongoCollection.FindOneAndUpdateAsync(filterUserName, update);
        }

        #region UpdateDocumentValue overloads
        /// <summary>
        /// Updates the document value for the given field.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="value">Value to change.</param>
        public static void UpdateDocumentValue(ulong user, string field, string value) {
            var filter = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

        /// <summary>
        /// Updates the document value for the given field.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="value">Value to change.</param>
        public static void UpdateDocumentValue(ulong user, string field, int value) {
            var filter = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

        /// <summary>
        /// Updates the document value for the given field.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="value">Value to change.</param>
        public static void UpdateDocumentValue(ulong user, string field, long value) {
            var filter = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }

        /// <summary>
        /// Updates the document value for the given field.
        /// </summary>
        /// <param name="user">User's document to find.</param>
        /// <param name="field">Field to update in the document.</param>
        /// <param name="value">Value to change.</param>
        public static void UpdateDocumentValue(ulong user, string field, bool value) {
            var filter = FilterMongoDocument("_id", user);
            var update = new BsonDocument("$set", new BsonDocument { { field, value } });
            MyMongoCollection.FindOneAndUpdateAsync(filter, update);
        }
        #endregion
    }
}

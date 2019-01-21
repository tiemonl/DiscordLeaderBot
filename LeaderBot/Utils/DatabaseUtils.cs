using System;
using System.Runtime.InteropServices;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LeaderBot.Utils {
    public class DatabaseUtils {
        public DatabaseUtils() {
        }

        public static IMongoDatabase GetDatabase { get; private set; }
        public static IMongoCollection<BsonDocument> GetMongoCollection { get; private set; }
        public static MongoClient GetMongoClient { get; private set; }


        /// <summary>
        /// Gets the mongo collection and sets up the connection.
        /// I am only passing in the Database name, because I am using only one database currently.
        /// </summary>
        /// <returns>The mongo collection</returns>
        public static void SetupMongoDatabase() {
            string connectionString = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                connectionString = Resources.mongoconnectionserver;
            else {
                connectionString = Resources.mongoworkconnection;
            }
            GetMongoClient = new MongoClient(connectionString);
            GetDatabase = GetMongoClient.GetDatabase("Leaderbot");

        }

        public static void ChangeCollection(string collectionName) {
            GetMongoCollection = GetDatabase.GetCollection<BsonDocument>(collectionName);
        }

        public static BsonDocument FindMongoDocument(string field, string criteria) {
            var result = DatabaseUtils.GetMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
            return result;
        }

        public static BsonDocument FindMongoDocument(string field, ulong criteria) {
            var result = DatabaseUtils.GetMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
            return result;
        }

        public static BsonDocument FindMongoDocument(string field, int criteria) {
            var result = DatabaseUtils.GetMongoCollection.Find(FilterMongoDocument(field, criteria)).FirstOrDefault();
            return result;
        }

        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, string criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }

        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, ulong criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }

        public static FilterDefinition<BsonDocument> FilterMongoDocument(string field, int criteria) {
            var filter = Builders<BsonDocument>.Filter.Eq(field, criteria);
            return filter;
        }
    }
}

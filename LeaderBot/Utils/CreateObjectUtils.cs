using Discord.WebSocket;
using MongoDB.Bson;

namespace LeaderBot.Utils
{
    class CreateObjectUtils
    {

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

		public static void CreateNewDailyPoints(string date) {
			var document = new BsonDocument {
				{ "_id", date},
				{ "users",  new BsonArray{ } }
				};
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

	}
}

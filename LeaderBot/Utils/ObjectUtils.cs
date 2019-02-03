using Discord;
using LeaderBot.Competition;
using LeaderBot.Objects;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace LeaderBot.Utils
{
    class ObjectUtils
    {
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
				CreateObjectUtils.CreateNewDailyPoints(criteria);
				doc = DatabaseUtils.FindMongoDocument(field, criteria, "dailyPoints");
			}
			if (doc != null) {
				dailyPoints = BsonSerializer.Deserialize<DailyPoints>(doc);
			} else {
				Logger.Log(new LogMessage(LogSeverity.Error, $"{typeof(Util).Name}.GetDailyPoints", "Could not find date!"));
			}
			return dailyPoints;
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
	}
}

namespace LeaderBot {
	public class UserInfo {
		public long _id { get; set; }
		public string name { get; set; }
		public bool isBetaTester { get; set; }
		public string dateJoined { get; set; }
		public int numberOfMessages { get; set; }
		public int reactionCount { get; set; }
		public int experience { get; set; }
		public int points { get; set; }
		public int winCoinflipStreak { get; set; }
		public int loseCoinflipStreak { get; set; }
		public string[] roles { get; set; }
		public int totalAttack { get; set; }
		public int totalDefense { get; set; }

		public UserInfo() { }
	}
}

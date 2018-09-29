
namespace LeaderBot {
	public class UserInfo {
		public string Name { get; set; }
		public bool IsBetaTester { get; set; }
		public string DateJoined { get; set; }
		public int NumberOfMessages { get; set; }
		public int ReactionCount { get; set; }
		public int Experience { get; set; }
		public int Points { get; set; }
		public int WinCoinflipStreak { get; set; }
		public int LoseCoinflipStreak { get; set; }
		public string[] Roles { get; set; }

		public UserInfo() { }
	}
}

using System;
namespace LeaderBot.Objects {
	public class Shop {
		public long _id { get; set; }
		public string name { get; set; }
		public int attack { get; set; }
		public int defence { get; set; }
		public int cost { get; set; }
		public int levelRequirement { get; set; }
		public Shop() {
		}
	}
}

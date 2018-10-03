using System;
namespace LeaderBot.Objects {
	public class Shop {
		public int _id { get; set; }
		public string Name { get; set; }
		public int Attack { get; set; }
		public int Defence { get; set; }
		public int Cost { get; set; }
		public int LevelRequirement { get; set; }
		public Shop() {
		}
	}
}

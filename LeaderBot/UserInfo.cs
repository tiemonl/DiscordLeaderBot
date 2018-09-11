using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace LeaderBot {
	class UserInfo {
		public string Name { get; set; }
		public bool IsBetaTester { get; set; }
		public string DateJoined { get; set; }
		public int NumberOfMessages { get; set; }
		public int ReactionCount { get; set; }
        public int Experience { get; set; }
        public int Points { get; set; }

		public UserInfo() { }
	}
}

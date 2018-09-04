using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LeaderBot
{
    class UserInfo
    {
		public string Name { get; }
		public bool IsBetaTester { get; }
		public string DateJoined { get; }
		public int NumberOfMessages { get; set; }

		public UserInfo(){}
	}
}

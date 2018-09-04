using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace LeaderBot
{
    class UserInfo
    {
        public string Name { get; set; }
        public bool IsBetaTester { get; set; }
        public string DateJoined { get; set; }
		public int NumberOfMessages { get; set; }

        //(LVL / 50)^2 * 800000 * ((LVL / 100) + 0.15) level formula i will use

        public UserInfo(){}
	}
}

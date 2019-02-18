using System.Collections.Generic;

namespace LeaderBot.Objects {
    public class PointBank {
        public string _id { get; set; }
        public int currentCredits { get; set; }
        public int maxCredits { get; set; }
        public int minWithdrawal { get; set; }
        public decimal interestRate { get; set; }
        public Dictionary<string, int> currentLoans { get; set; }
    }
}

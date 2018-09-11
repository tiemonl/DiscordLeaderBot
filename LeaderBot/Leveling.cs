using System;
using System.Collections.Generic;

namespace LeaderBot {
	public class Leveling {
		public Leveling() {
		}

		public double getExperience(double currentLvl) {
			double experience = 0;
			experience = Math.Round(Math.Pow((currentLvl * 50), 1.3));
			return experience;
		}

		public double getLevel(double currentExp) {
			double level = 0;
			level = Math.Round(Math.Pow(currentExp, 1 / 1.3) / 50);
			return level;
		}

        public void addExperience(int expToAdd){

        }
	}
}

using System;
using System.Collections.Generic;

namespace LeaderBot
{
    public class Leveling
    {
        public Leveling()
        {
        }

        //(LVL / 50)^2 * 800000 * ((LVL / 100) + 0.15).

        private double currentExperience;
        private double currentLevel;

        public double addXP(){
            return 1;
        }



        public double getExperience(double currentLvl){
            double experience = 0;
            experience = Math.Round(Math.Pow(currentLvl / 50, 2) * 800000 * ((currentLvl / 100) + .15));
            return experience;
        }
    }
}

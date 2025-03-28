using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType{Hunt, Defend, Mine, Deliver};
public enum JobTarget{Faction1, Faction2, Pirate, Solo, Asteroid, Self};
public enum RepType{Faction1, Faction2, Pirate, Self};

namespace Model
{
    [CreateAssetMenu(fileName = "Job", menuName = "ScriptableObjects/Job", order = 0)]
    public class Job : ScriptableObject
    {
        public string jobName = "Placeholder Name";
        public string jobDescription = "Placeholder Description";
        
        public JobType jobType = JobType.Defend;

        public Scenario  scenario = null;

        public int rewardCoins = 1;
        public RepType rewardType = RepType.Faction1; 
        public int rewardRep = 1;

        public JobTarget jobTarget = JobTarget.Faction1;
        public int quantity = 1;

        public int dangerValue = 0;

        //public RepType minRepType = RepType.Faction1;
        //public int minRep = 0;     
        
        //public RepType maxRepType = RepType.Faction2; 
        //public int maxRep = 2;     

       
    }
    public static class JobUtil
    {
        public static string ToTag(this JobTarget target)
        {
            switch (target)
            {
                case JobTarget.Faction1:
                    return "Faction1";
                case JobTarget.Faction2:
                    return "Faction2";
                case JobTarget.Pirate:
                    return "PirateShip";
                case JobTarget.Solo:
                    return "SoloShip";
                case JobTarget.Asteroid:
                    return "Asteroid";
                case JobTarget.Self:
                    return "Player";
                default:
                    return "Other";
            }
        }
    }
}



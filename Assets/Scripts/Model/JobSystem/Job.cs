using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType{Hunt, Defend, Mine, Deliver};
public enum JobTarget{Colonial, Earth, Pirate, Solo, Asteroid, Self};
public enum RepType{Colonial, Earth, Pirate, Self};

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
        public RepType rewardType = RepType.Colonial; 
        public int rewardRep = 1;

        public JobTarget jobTarget = JobTarget.Colonial;
        public int quantity = 1;

        public Faction targetFaction = null;
        public Faction allyFaction = null;

        public int dangerValue = 0;
 

       
    }
    public static class JobUtil
    {
        public static string ToTag(this JobTarget target)
        {
            switch (target)
            {
                case JobTarget.Colonial:
                    return "Faction1";
                case JobTarget.Earth:
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



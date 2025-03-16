using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JobType{Hunt, Defend, Mine, Deliver};
public enum JobTarget{Faction1, Faction2, Pirate, Ore, Cargo, Other};
public enum RepType{Faction1, Faction2, Pirate, Self};

[CreateAssetMenu(fileName = "JobModel", menuName = "ScriptableObjects/JobModel", order = 0)]
public class JobModel : ScriptableObject
{
    public string jobName = "Placeholder Name";
    public JobType jobType = JobType.Defend;

    public MapModel  mapModel = null;

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

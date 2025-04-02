using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Controller;
using Model;

public enum ReputationStatus
    {
        Neutral,
        Friendly,
        Enemy
    }

public class PlayerReputation : MonoBehaviour
{   
        
    public List<Reputation> reputations = new List<Reputation>();
    public int coins;

    public static PlayerReputation Inst { get; private set; } //Singleton
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }

        foreach (RepType rep in System.Enum.GetValues(typeof(RepType)))
        {
            Debug.LogWarning("ForTests - Change back to 0");
            reputations.Add(new Reputation (rep, 1000));
        }
    }
    
    public float GetReputation(Faction faction)
    {
        return 0.0f;
    }

    public ReputationStatus GetReputationStatus(Faction factionA, Faction factionB)
    {
        if (factionA.factionType == Faction.FactionType.Pirates || factionB.factionType == Faction.FactionType.Pirates)
        {
            return ReputationStatus.Enemy;
        }

        if (factionA.factionType == Faction.FactionType.Solo || factionB.factionType == Faction.FactionType.Solo)
        {
            return ReputationStatus.Neutral;
        }

        Job job = JobController.Inst.currJob;
        if ((job.targetFaction == factionA && job.allyFaction == factionB) || 
            (job.targetFaction == factionB && job.allyFaction == factionA))
        {
            return ReputationStatus.Enemy;
        }

        return ReputationStatus.Neutral;
    }

    public ReputationStatus GetReputationStatus(Faction faction)
    {
        Job job = JobController.Inst.currJob;
        if (job.allyFaction == faction)
        {
            return ReputationStatus.Friendly;
        }

        return ReputationStatus.Neutral;
    }

    public void ChangeReputation(RepType _type, int _value)
    {
        Reputation rep = reputations.Find(i => i.type == _type);
        if (rep != null)
            rep.ChangeValue(_value);
        else
            Debug.LogWarning($"Item of type {_type} not found.");
    }
}

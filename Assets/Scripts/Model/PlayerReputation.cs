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


[CreateAssetMenu(fileName = "PlayerReputation", menuName = "ScriptableObjects/PlayerReputation", order = 0)]
public class PlayerReputation : ScriptableObject
{
    public Faction player;
    public List<Reputation> reputations = new List<Reputation>();
    public int coins;

    public int limit = 100;

    public void Reset()
    {
        foreach (Reputation rep in reputations)
        {
            rep.value = 0;
        }
        coins = 0;
    }

    public float GetReputation(Faction faction)
    {
        return 0.0f;
    }

    public ReputationStatus GetReputationStatus(Faction factionA, Faction factionB)
    {
        if ((factionA.factionType == Faction.FactionType.Pirates && factionB.factionType != Faction.FactionType.Pirates)
            || (factionB.factionType == Faction.FactionType.Pirates && factionA.factionType != Faction.FactionType.Pirates))
        {
            return ReputationStatus.Enemy;
        }

        if (factionA.factionType == Faction.FactionType.Solo || factionB.factionType == Faction.FactionType.Solo)
        {
            return ReputationStatus.Neutral;
        }

        Job job = JobController.Inst.currJob;
        if ((job.enemyFaction == factionA && job.allyFaction == factionB) ||
            (job.enemyFaction == factionB && job.allyFaction == factionA))
        {
            return ReputationStatus.Enemy;
        }

        if (factionB == player)
        {
            return GetReputationStatus(factionA);
        }

        return ReputationStatus.Neutral;
    }

    public ReputationStatus GetReputationStatus(Faction faction)
    {
        Reputation reputation = reputations.Find((x) => x.fac == faction);
        if (reputation == null)
        {
            return ReputationStatus.Neutral;
        }

        if (reputation.value >= 100)
        {
            return ReputationStatus.Friendly;
        }
        else if (reputation.value <= -100)
        {
            return ReputationStatus.Enemy;
        }

        return ReputationStatus.Neutral;
    }

    public void ChangeReputation(Faction _fac, int _value)
    {
        Reputation rep = reputations.Find(i => i.fac == _fac);
        if (rep != null)
            rep.ChangeValue(_value);
        else
            Debug.LogWarning($"Item of faction {_fac} not found.");
    }
}

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
    
    public List<Reputation> reputations = new List<Reputation>();
    public int coins;
    
    public float GetReputation(Faction faction)
    {
        return 0.0f;
    }
    
    public ReputationStatus GetReputationStatus(Faction faction)
    {
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

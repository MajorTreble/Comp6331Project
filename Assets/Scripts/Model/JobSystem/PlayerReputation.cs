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

    //public static PlayerReputation Inst { get; private set; } //Singleton
    private void Awake()
    {
        Faction[] allFac = Resources.LoadAll<Faction>("Scriptable/Faction");
        if (allFac == null || allFac.Length == 0)
        {
            Debug.LogError("No faction found in the specified path.");
            return;
        }
        foreach (Faction fac in allFac)
        {
            Debug.LogWarning("ForTests - Change back to 0");
            reputations.Add(new Reputation (fac, 789));
        }
    }
    
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

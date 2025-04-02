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

        public float GetReputation(Faction faction)
        {
            return 0.0f;
        }
        
        public ReputationStatus GetReputationStatus(Faction faction)
        {
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

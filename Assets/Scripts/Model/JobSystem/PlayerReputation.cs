using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Controller;

namespace Model
{
    public enum ReputationStatus
    {
        Neutral,
        Friendly,
        Enemy
    }

    [CreateAssetMenu(fileName = "Player Reputation", menuName = "ScriptableObjects/Player Reputation", order = 0)]
    public class PlayerReputation : ScriptableObject
    {
        public float allyReputationThreshold = 70f; // Reputation level required to ally with the player

        public List<Reputation> reputations = new List<Reputation>();
        public int coins;

        public void Start()
        {
            reputations.Add(new Reputation (RepType.Colonial, 0));
            reputations.Add(new Reputation (RepType.Earth, 0));
            reputations.Add(new Reputation (RepType.Pirate, 0));
            reputations.Add(new Reputation (RepType.Self, 0));     
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
}
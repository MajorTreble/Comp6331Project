// AIBehavior.cs
using UnityEngine;

namespace Model
{

    [CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior")]
    public class AIBehavior : ScriptableObject
    {
        public enum Faction { Colonial, Earth, Pirates, Solo }

        [Header("General Settings")]
        public Faction faction;
        public float aggressionLevel; // How aggressive this faction is
        public float reputationThreshold; // Reputation level required to avoid attacks
        public float allyReputationThreshold = 70f; // Reputation level required to ally with the player
        public float allyAssistRange = 20f; // Range within which the AI will assist the player
        [Range(0, 2)] public float missionResponseAggression = 1f;
        public float detectionRadius = 25f;

        [Header("Movement")]
        public float roamSpeed;
        public float chaseSpeed;
        public float rotationSpeed;

        [Header("Combat")]
        public float attackRange;
        public float attackCooldown;

        [Header("Aggression Tuning")]
        public float aggressionCalmValue = 1f;
        public float aggressionAverageValue = 4f;
        public float aggressionAggressiveValue = 10f;

    }

}
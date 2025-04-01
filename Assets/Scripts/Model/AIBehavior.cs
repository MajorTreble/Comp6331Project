// AIBehavior.cs
using UnityEngine;

namespace Model
{

    [CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior")]
    public class AIBehavior : ScriptableObject
    {

        [Header("General Settings")]
        public float aggressionLevel; // How aggressive this faction is
        public float reputationThreshold; // Reputation level required to avoid attacks
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
    }

}
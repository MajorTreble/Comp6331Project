using System.Collections;
using Manager;
using UnityEngine;

namespace Model.AI.Environment
{
    

    public class BlackHoleFuzzyLogic : MonoBehaviour
    {
        [Header("Inputs")]
        public int orbitingMass = 0; // Number of orbiting objects
        public float averageProximity = 20f; // Distance between magnetic fragments

        [Header("Output")]
        [Range(0, 100)]
        public float blackHoleProbability = 0f;

        [Header("Mass Thresholds")]
        public int lowMassThreshold = 12;
        public int mediumMassThreshold = 25;

        [Header("Proximity Thresholds")]
        public float farProximityThreshold = 25f;
        public float mediumProximityThreshold = 15f;

        public GameObject blackHolePrefab;

        public MagneticFragmentGroupAI fragmentGroup;


        public void Awake()
        {
            if (blackHolePrefab == null)
            {
                Debug.LogError("[BlackHoleFuzzyLogic] Blackhole prefab is null.");
            }

            if (fragmentGroup == null)
                fragmentGroup = GetComponentInParent<MagneticFragmentGroupAI>();
        }

        void Update()
        {
            EvaluateFuzzyLogic();
        }

        void EvaluateFuzzyLogic()
        {
            float massScore = FuzzifyMass(orbitingMass);
            float proximityScore = FuzzifyProximity(averageProximity);

            // weighted average
            blackHoleProbability = (massScore * 0.6f) + (proximityScore * 0.4f);
            Debug.Log($"[BlackHoleFuzzyLogic] number of orbiting mass: {orbitingMass}");
            if (blackHoleProbability > 80f)
            {
                Debug.Log("[BlackHoleFuzzyLogic] High collapse chance! Triggering black hole...");
                TryTriggerBlackHole();
            }
        }

        float FuzzifyMass(int mass)
        {
            if (mass <= lowMassThreshold) return 10f;     // Low mass → low chance
            if (mass <= mediumMassThreshold) return 50f;  // Medium mass
            return 90f;                                   // High mass
        }

        float FuzzifyProximity(float proximity)
        {
            if (proximity > farProximityThreshold) return 10f;       // Too far
            if (proximity > mediumProximityThreshold) return 50f;    // Medium
            return 90f;                                              // Close proximity
        }

        void TryTriggerBlackHole()
        {
            Debug.Log("[BlackHoleFuzzyLogic] Black Hole Triggered!");
            if (fragmentGroup == null) return;

            Vector3 spawnPos = fragmentGroup.GetFragmentCentroid();

            GameObject bh = Instantiate(blackHolePrefab, spawnPos, Quaternion.identity);
            Destroy(gameObject);
        }

    }

}

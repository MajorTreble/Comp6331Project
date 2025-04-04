﻿using System.Collections;
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

        private GameObject blackHolePrefab;

        public MagneticFragmentGroupAI fragmentGroup;


        public void Awake()
        {
            blackHolePrefab = GameManager.Instance.blackHolePrefab;

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
            if (mass <= 12) return 10f;     // Low mass → low chance
            if (mass <= 25) return 50f;     // Medium mass
            return 90f;                    // High mass
        }

        float FuzzifyProximity(float proximity)
        {
            if (proximity > 25f) return 10f;     // Too far
            if (proximity > 15f) return 50f;     // Medium
            return 90f;                          // Close proximity
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

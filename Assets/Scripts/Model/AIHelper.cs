using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using Manager;
using Model.AI;

namespace Model
{
    public class AIHelper
    {
        public static bool IsTargetInRange(AIShip ship)
        {
            Debug.Assert(ship != null);

            if (ship.targetShip == null)
			{
                return false;
			}

            return IsTargetInRange(ship, ship.targetShip, ship.detectionRadius);
        }

        public static bool IsTargetInRange(Ship ship, Ship target, float detectionRadius)
        {
            Debug.Assert(ship != null && target != null, $"{MethodBase.GetCurrentMethod().Name} {ship} {target}");

            float distanceToPlayer = Vector3.Distance(ship.transform.position, target.transform.position);
            return distanceToPlayer < detectionRadius;
        }

        public static bool IsPlayerFriendly(Faction faction)
        {
            Debug.Assert(faction != null, $"{MethodBase.GetCurrentMethod().Name} {faction}");

            PlayerReputation reputation = GameManager.Instance.reputation;
            Debug.Assert(reputation != null);

            return reputation.GetReputationStatus(faction) == ReputationStatus.Friendly;
        }

        public static bool IsEnemy(Faction a, Faction b)
        {
            Debug.Assert(a != null && b != null, $"{MethodBase.GetCurrentMethod().Name} {a} {b}");

            PlayerReputation reputation = GameManager.Instance.reputation;
            Debug.Assert(reputation != null);

            return reputation.GetReputationStatus(a, b) == ReputationStatus.Enemy;
        }

        private static bool IsPlayerEnemy(Faction faction)
        {
            Debug.Assert(faction != null, $"{MethodBase.GetCurrentMethod().Name} {faction}");

            PlayerReputation reputation = GameManager.Instance.reputation;
            Debug.Assert(reputation != null);

            return reputation.GetReputationStatus(faction) == ReputationStatus.Enemy;
        }

        public static bool ShouldAttackPlayer(AIShip self, GameObject player, Job job)
        {
            Debug.Assert(self != null && player != null && job != null, $"{MethodBase.GetCurrentMethod().Name} {self} {player} {job}");

            return IsPlayerEnemy(self.faction);
        }

        public static bool IsMissionTarget(Ship ship)
        {
            Debug.Assert(ship != null, $"{MethodBase.GetCurrentMethod().Name} {ship}");

            return ship.IsJobTarget();
        }

        // Membership functions (linear, as suggested)
        public static float LeftShoulderMembership(float value, float x, float y)
        {
            if (value <= x) return 1f;
            if (value >= y) return 0f;
            return (y - value) / (y - x);
        }

        public static float RightShoulderMembership(float value, float x, float y)
        {
            if (value <= x) return 0f;
            if (value >= y) return 1f;
            return (value - x) / (y - x);
        }

        public static Relationship EvaluateRelationship(Faction faction)
        {
            float rep = GameManager.Instance.reputation.GetReputation(faction); // 0 to 100

            float enemyThresholdLow = 30f;
            float enemyThresholdHigh = 50f;
            float friendlyThresholdLow = 50f;
            float friendlyThresholdHigh = 70f;

            float enemyMembership = LeftShoulderMembership(rep, enemyThresholdLow, enemyThresholdHigh);
            float friendlyMembership = RightShoulderMembership(rep, friendlyThresholdLow, friendlyThresholdHigh);

            if (enemyMembership > 0.5f)
                return Relationship.Enemy;
            else
                return Relationship.Neutral; // You could expand this to include Friendly later
        }

        public static int CountNearbyFriendlies(AIShip self, float distance = 100.0f)
        {
            Debug.Assert(self != null, $"{MethodBase.GetCurrentMethod().Name} {self}");

            int count = 0;
            foreach (AIShip other in SpawningManager.Instance.shipList)
            {
                if (other == self) continue;
                if (other.faction == self.faction)
                {
                    float d = Vector3.Distance(self.transform.position, other.transform.position);
                    if (d <= distance) count++;
                }
            }
            return count;
        }

        public static int CountNearbyEnemies(AIShip self, float distance = 100.0f)
        {
            Debug.Assert(self != null, $"{MethodBase.GetCurrentMethod().Name} {self}");

            int count = 0;
            foreach (AIShip other in SpawningManager.Instance.shipList)
            {
                if (other == self) continue;
                if (other.faction != self.faction && IsEnemy(self.faction, other.faction))
                {
                    float d = Vector3.Distance(self.transform.position, other.transform.position);
                    if (d <= distance) count++;
                }
            }

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            Debug.Assert(player != null);

            if (AIHelper.IsPlayerEnemy(self.faction))
            {
                float d = Vector3.Distance(self.transform.position, player.position);
                if (d <= distance) count++;
            }
            return count;
        }

        public static float EvaluateAggressionLevel(AIShip self)
        {
            Debug.Assert(self != null, $"{MethodBase.GetCurrentMethod().Name} {self}");

            int friendlyCount = CountNearbyFriendlies(self);
            int enemyCount = CountNearbyEnemies(self);
            float selfStrength = self.health / self.oriData.maxHealth;

            // Define thresholds for counts (these can be tuned)
            float friendHighThreshold = 5f;
            float enemyHighThreshold = 5f;
            float friendlyHigh = Mathf.Clamp01(friendlyCount / friendHighThreshold);
            float enemyHigh = Mathf.Clamp01(enemyCount / enemyHighThreshold);

            // Fuzzy rules:
            // If many enemies and few friendlies, be aggressive.
            float ruleAggressive = Mathf.Min(1f - friendlyHigh, enemyHigh);
            // Otherwise, if many friendlies and few enemies, be calm.
            float ruleCalm = Mathf.Min(friendlyHigh, 1f - enemyHigh);
            // Average in other cases.
            float ruleAverage = 1f - Mathf.Abs(enemyHigh - friendlyHigh);

            float calmValue = self.behavior.aggressionCalmValue;
            float avgValue = self.behavior.aggressionAverageValue;
            float aggressiveValue = self.behavior.aggressionAggressiveValue;

            float weightedSum = ruleCalm * calmValue + ruleAverage * avgValue + ruleAggressive * aggressiveValue;
            float totalWeight = ruleCalm + ruleAverage + ruleAggressive;
            float aggression = (totalWeight > 0f) ? weightedSum / totalWeight : avgValue;
            return aggression;
        }
    }
}
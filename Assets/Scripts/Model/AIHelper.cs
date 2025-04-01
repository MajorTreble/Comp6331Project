using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Manager;
using Model.AI;

namespace Model
{
    public class AIHelper
    {
        public static bool IsTargetInRange(AIShip ship)
        {
            if (ship == null) return false;

            return IsTargetInRange(ship, ship.targetShip, ship.detectionRadius);
        }

        public static bool IsTargetInRange(Ship ship, Ship target, float detectionRadius)
        {
            if (ship == null || target == null) return false;

            float distanceToPlayer = Vector3.Distance(ship.transform.position, target.transform.position);
            return distanceToPlayer < detectionRadius;
        }

        public static bool ShouldAllyWithPlayer(GameObject player, Faction faction)
        {
            PlayerReputation reputation = GameManager.Instance.reputation;

            if (player == null || reputation == null || faction == null) return false;

            return reputation.GetReputationStatus(faction) == ReputationStatus.Friendly;
        }

        public static bool ShouldAttackPlayer(AIShip ship, GameObject player, Job job, Faction faction)
        {
            PlayerReputation reputation = GameManager.Instance.reputation;

            if (player == null || reputation == null || faction == null) return false;

            // Always prioritize ally status
            if (ShouldAllyWithPlayer(player, faction)) return false;

            // Get current job context3
            bool isMissionTarget = IsMissionTarget(ship);
            bool isMissionAlly = IsMissionAlly(faction);

            // Mission-based behavior
            if (job != null)
            {
                // Strategic responses based on job type
                switch (job.jobType)
                {
                    case JobType.Hunt when isMissionTarget:
                        // If we're the hunt target, become aggressive
                        return true;

                    case JobType.Defend when isMissionTarget:
                        // If we're the defend target, protect ourselves
                        return reputation.GetReputationStatus(faction) != ReputationStatus.Friendly;

                    case JobType.Mine:
                        // Pirates attack mining operations
                        return reputation.GetReputationStatus(faction) == ReputationStatus.Enemy; ;

                    case JobType.Deliver when isMissionTarget:
                        // Intercept delivery missions targeting our faction
                        return true;
                }

                // Faction response to mission alignment
                if (isMissionAlly)
                {
                    // Support player if we're the rewarded faction
                    return false;
                }

                if (job.allyFaction == faction)
                {
                    // Pirates support player if mission benefits them
                    return false;
                }
            }

            // Default reputation-based behavior
            return reputation.GetReputationStatus(faction) == ReputationStatus.Enemy;
        }

        public static bool IsMissionTarget(Ship ship)
        {
            if (ship == null) { return false; }

            return ship.IsJobTarget();
        }

        public static bool IsMissionAlly(Faction faction)
        {
            PlayerReputation reputation = GameManager.Instance.reputation;

            if (reputation == null || faction == null) { return false; }

            return reputation.GetReputationStatus(faction) == ReputationStatus.Friendly;
        }
    }
}
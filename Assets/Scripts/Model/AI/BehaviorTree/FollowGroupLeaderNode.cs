using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI.BehaviorTree;

namespace Model.AI.BehaviorTree
{

    public class FollowGroupLeaderNode : Node
    {
        protected AIShip aiShip = null;
        protected float distance = 20.0f;

        public FollowGroupLeaderNode(AIShip aiShip, float distance = 20.0f)
        {
            this.aiShip = aiShip;
            this.distance = distance;
        }

        public override NodeState Evaluate()
        {
            if (aiShip == null || aiShip.group == null)
            {
                return NodeState.FAILURE;
            }

            AIShip leader = aiShip.group.leader;
            if (leader == null)
            {
                return NodeState.FAILURE;
            }

            aiShip.steeringAgent.trackedTarget = leader.steeringAgent;
            int followerIndex = leader.group.ships.IndexOf(aiShip);
            int followPos = (int)((followerIndex + 1) / 2);
            float side = ((followerIndex % 2) == 0 ? 1.0f : -1.0f);
            aiShip.steeringAgent.targetOffset = new Vector3(distance * followPos * side, 0, -distance * followPos);

            return NodeState.SUCCESS;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI.BehaviorTree;

namespace Model.AI.BehaviorTree
{
    public class AIShipBehaviorTree : global::AI.BehaviorTree.BehaviorTree
    {
        public AIShip aiShip = null;

        protected override Node SetupTree()
        {
            Node root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new Condition( () => aiShip.currentState != AIState.Seek ),
                    new Condition( () => AIHelper.IsTargetInRange(aiShip) ),

                    new SetAIStateNode(aiShip, AIState.Seek)
                }),
                new Sequence(new List<Node>
                {
                    new Condition( () => aiShip.currentState != AIState.Formation ),
                    new Condition( () => aiShip.group != null ),
                    new Condition( () => aiShip.group.leader != aiShip ),
                    new Condition( () => aiShip.group.groupMode == GroupMode.Formation ),

                    new SetAIStateNode(aiShip, AIState.Formation),
                    new FollowGroupLeaderNode(aiShip)
                }),
                new Sequence(new List<Node>
                {
                    new Condition( () => aiShip.currentState != AIState.Formation ),
                    new SetAIStateNode(aiShip, AIState.Roam)
                })
            }); ;

            return root;
        }
    }
}
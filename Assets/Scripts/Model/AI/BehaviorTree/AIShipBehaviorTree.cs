using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI.BehaviorTree;
using Model.AI;

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
                    new Condition( () => aiShip.currentLKP != null && aiShip.currentLKP.visibility == LKPVisibility.Seen ),
                    new Condition( () => aiShip.target != null ),

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
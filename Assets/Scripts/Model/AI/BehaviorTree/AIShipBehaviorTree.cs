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
					new Condition( () => aiShip.IsPlayerInRange() ),
					new SetAIStateNode(aiShip, AIState.Seek)
				}),
				new SetAIStateNode(aiShip, AIState.Roam)
			});

			return root;
		}
	}
}
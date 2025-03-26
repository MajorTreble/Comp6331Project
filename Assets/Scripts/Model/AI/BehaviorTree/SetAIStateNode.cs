using System.Collections;
using System.Collections.Generic;

using AI.BehaviorTree;

namespace Model.AI.BehaviorTree
{

    public class SetAIStateNode : Node
    {
        protected AIShip aiShip = null;
        protected AIState aiState = AIState.None;

        public SetAIStateNode(AIShip aiShip, AIState aiState)
		{
            this.aiShip = aiShip;
            this.aiState = aiState;
		}

        public override NodeState Evaluate()
		{
            if (aiShip == null)
			{
                return NodeState.FAILURE;
			}

            aiShip.requestState = aiState;

            return NodeState.SUCCESS;
        }
    }

}

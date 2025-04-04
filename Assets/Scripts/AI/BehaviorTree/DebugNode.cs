using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AI.BehaviorTree;

namespace AI.BehaviorTree
{

    public class DebugNode : Node
    {
        private string message;

        public DebugNode(string message)
        {
            this.message = message;
        }

        public override NodeState Evaluate()
        {
            Debug.Log(message);

            return NodeState.SUCCESS;
        }

    }

}

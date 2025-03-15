using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    public abstract class BehaviorTree
    {
        private Node _root = null;

        public void Initialize()
        {
            _root = SetupTree();
        }

        public NodeState Evaluate()
        {
            if (_root != null)
			{
                return _root.Evaluate();
            }

            return NodeState.NONE;
        }

        protected abstract Node SetupTree();

    }

}

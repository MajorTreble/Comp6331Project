using System;
using System.Collections;
using System.Collections.Generic;

namespace AI.BehaviorTree
{

    public class Condition : Node
    {
        readonly Func<bool> predicate;

        public Condition() : base() { }
        public Condition(Func<bool> predicate) : base() { this.predicate = predicate; }

        public override NodeState Evaluate()
        {
            return predicate.Invoke() ? NodeState.SUCCESS : NodeState.FAILURE;
        }

    }

}

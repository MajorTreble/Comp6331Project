using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using AI.BehaviorTree;

public class BehaviorTreeTest
{
    class StubBT : BehaviorTree
	{
        protected override Node SetupTree()
		{
            Node root = new Selector(new List<Node>
            {
                new Sequence(new List<Node>
				{
                    new Condition( () => true )
				})
            });

            return root;
		}
	}

    // A Test behaves as an ordinary method
    [Test]
    public void TestBehaviorTree()
    {
        BehaviorTree bt = new StubBT();
        bt.Initialize();
        Assert.That(bt.Evaluate(), Is.EqualTo(NodeState.SUCCESS));
    }
}

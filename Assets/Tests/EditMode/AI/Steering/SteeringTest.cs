using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using AI.Steering;

public class SteeringTest
{
    [Test]
    public void TestSteering()
    {
        GameObject gameObject = new GameObject();
        gameObject.AddComponent<SteeringAgent>();
        SteeringAgent agent = gameObject.GetComponent<SteeringAgent>();
        agent.owner = gameObject;

        agent.Awake();

        agent.maxSpeed = 1.0f;
        agent.TargetPosition = new Vector3(10.0f, 0.0f, 0.0f);

        agent.movements.Add(new Arrive());
        agent.movements.Add(new CollisionAvoidance());
        agent.movements.Add(new Evade());
        agent.movements.Add(new FaceAway());
        agent.movements.Add(new Flee());
        agent.movements.Add(new LookWhereYouAreGoing());
        agent.movements.Add(new Pursue());
        agent.movements.Add(new Seek());
        agent.movements.Add(new Wander());

        agent.Update();
        Assert.That(gameObject.transform.position.magnitude, Is.Not.Zero);
    }
}

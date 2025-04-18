using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Model;
using Model.AI;

public class ShipTest
{
    public PlayerShip playerShip;
    public AIShip aiShip;

    [SetUp]
    public void Setup()
    {
        ShipLoader shipLoader = Resources.Load<ShipLoader>("Scriptable/ShipLoader");

        playerShip = GameObject.Instantiate(shipLoader.playerShipPrefab, new Vector3(0f, 0f, -50f), Quaternion.identity).GetComponent<PlayerShip>();
        aiShip = GameObject.Instantiate(shipLoader.aiShipPrefab, Vector3.zero, Quaternion.AngleAxis(180f, Vector3.up)).GetComponent<AIShip>();
    }

    [TearDown]
    public void TearDown()
    {
        playerShip.GetComponent<Collider>().enabled = false;
        aiShip.GetComponent<Collider>().enabled = false;

        GameObject.DestroyImmediate(playerShip);
        GameObject.DestroyImmediate(aiShip);
    }

    [UnityTest]
    public IEnumerator TestSpawning()
    {
        Assert.IsNotNull(playerShip);
        Assert.IsNotNull(aiShip);

        Assert.IsTrue(aiShip.currentState == AIState.Roam);

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestPlayerShipAttack()
    {
        playerShip.Attack();

        for(int i=0; i<16; ++i)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(aiShip.health < aiShip.oriData.maxHealth, $"{aiShip.health} < {aiShip.oriData.maxHealth}");
    }

    [UnityTest]
    public IEnumerator TestAIShipAttack()
    {
        aiShip.Attack();

        for (int i = 0; i < 16; ++i)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(playerShip.shields < playerShip.oriData.maxShields, $"{playerShip.shields} < {playerShip.oriData.maxShields}");
    }
}

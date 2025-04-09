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

        playerShip = GameObject.Instantiate(shipLoader.playerShipPrefab).GetComponent<PlayerShip>();
        aiShip = GameObject.Instantiate(shipLoader.aiShipPrefab).GetComponent<AIShip>();
    }

    [UnityTest]
    public IEnumerator TestSpawning()
    {
        Debug.Assert(playerShip != null);
        Debug.Assert(aiShip != null);

        Debug.Assert(aiShip.currentState == AIState.Roam);

        yield return null;
    }
}

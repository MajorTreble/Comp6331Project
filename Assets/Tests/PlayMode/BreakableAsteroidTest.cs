using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Model;
using Model.AI;
using Model.Environment;

public class BreakableAsteroidTest
{
    public PlayerShip playerShip;
    public BreakableAsteroid breakableAsteroid;

    [SetUp]
    public void Setup()
    {
        ShipLoader shipLoader = Resources.Load<ShipLoader>("Scriptable/ShipLoader");

        playerShip = GameObject.Instantiate(shipLoader.playerShipPrefab, new Vector3(0f, 0f, -50f), Quaternion.identity).GetComponent<PlayerShip>();
        breakableAsteroid = GameObject.Instantiate(shipLoader.breakableAsteroidPrefab, new Vector3(-3f, 0f, 0f), Quaternion.identity).GetComponent<BreakableAsteroid>();
    }

    [TearDown]
    public void TearDown()
    {
        playerShip.GetComponent<Collider>().enabled = false;
        breakableAsteroid.GetComponent<Collider>().enabled = false;

        GameObject.DestroyImmediate(playerShip);
        GameObject.DestroyImmediate(breakableAsteroid);
    }

    [UnityTest]
    public IEnumerator TestSpawning()
    {
        Assert.IsNotNull(playerShip);
        Assert.IsNotNull(breakableAsteroid);

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestPlayerShipAttack()
    {
        playerShip.Attack();

        for (int i = 0; i < 16; ++i)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(breakableAsteroid.health < 50f);
    }
}

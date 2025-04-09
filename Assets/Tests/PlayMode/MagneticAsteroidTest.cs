using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Model;
using Model.AI;
using Model.Environment;

public class MagneticAsteroidTest
{
    public PlayerShip playerShip;
    public MagneticAsteroid magneticAsteroid;

    [SetUp]
    public void Setup()
    {
        ShipLoader shipLoader = Resources.Load<ShipLoader>("Scriptable/ShipLoader");

        playerShip = GameObject.Instantiate(shipLoader.playerShipPrefab, new Vector3(0f, 0f, -50f), Quaternion.identity).GetComponent<PlayerShip>();
        magneticAsteroid = GameObject.Instantiate(shipLoader.magneticAsteroidPrefab, new Vector3(-3f, 0f, 0f), Quaternion.identity).GetComponent<MagneticAsteroid>();
    }

    [TearDown]
    public void TearDown()
    {
        playerShip.GetComponent<Collider>().enabled = false;

        if (magneticAsteroid)
        {
            magneticAsteroid.GetComponent<Collider>().enabled = false;
        }

        GameObject.DestroyImmediate(playerShip);
        GameObject.DestroyImmediate(magneticAsteroid);
    }

    [UnityTest]
    public IEnumerator TestSpawning()
    {
        Assert.IsNotNull(playerShip);
        Assert.IsNotNull(magneticAsteroid);

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

        Assert.IsTrue(magneticAsteroid.hasBroken);
    }
}

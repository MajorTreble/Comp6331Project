using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Model;
using Model.AI;
using Model.Environment;

public class MineableAsteroidTest
{
    public PlayerShip playerShip;
    public MineableAsteroid mineableAsteroid;

    public bool isDestroyed = false;

    [SetUp]
    public void Setup()
    {
        ShipLoader shipLoader = Resources.Load<ShipLoader>("Scriptable/ShipLoader");

        playerShip = GameObject.Instantiate(shipLoader.playerShipPrefab, new Vector3(0f, 0f, -50f), Quaternion.identity).GetComponent<PlayerShip>();
        mineableAsteroid = GameObject.Instantiate(shipLoader.mineableAsteroidPrefab).GetComponent<MineableAsteroid>();
        isDestroyed = false;

        Asteroid.OnDestroyed += OnDestroyed;
    }

    [TearDown]
    public void TearDown()
    {
        playerShip.GetComponent<Collider>().enabled = false;
        mineableAsteroid.GetComponent<Collider>().enabled = false;

        GameObject.DestroyImmediate(playerShip);
        GameObject.DestroyImmediate(mineableAsteroid);
    }

    [UnityTest]
    public IEnumerator TestSpawning()
    {
        Assert.IsNotNull(playerShip);
        Assert.IsNotNull(mineableAsteroid);

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

        Assert.IsTrue(mineableAsteroid.health < 50f);
    }

    [UnityTest]
    public IEnumerator TestPlayerShipAttackDestroyed()
    {
        playerShip.weapon_1.Setup(playerShip, 100f);
        playerShip.weapon_2.Setup(playerShip, 100f);

        playerShip.Attack();

        for (int i = 0; i < 16; ++i)
        {
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(isDestroyed);
    }

    public void OnDestroyed(Asteroid asteroid)
    {
        isDestroyed = true;
    }
}

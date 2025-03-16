using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip1_Spawner : MonoBehaviour
{
	public GameObject[] enemyPrefabs; // Array of enemy prefabs
	public int numberOfEnemies = 5;
	public float spawnRadius = 20f;

	void Start()
	{
		for (int i = 0; i < numberOfEnemies; i++)
		{
			SpawnEnemy();
		}
	}

	void SpawnEnemy()
	{
		Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
		randomPosition += transform.position;

		// Randomly select an enemy prefab
		GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
		Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
	}
}

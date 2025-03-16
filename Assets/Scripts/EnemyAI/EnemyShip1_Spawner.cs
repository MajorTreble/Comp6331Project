using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip1_Spawner : MonoBehaviour
{
	public GameObject[] enemyPrefabs; // Array of all enemy prefabs (Solo, Pirates, etc.)
	public GameObject faction1Prefab; // Single prefab for Faction 1 ships
	public GameObject faction2Prefab; // Single prefab for Faction 2 ships
	public int numberOfEnemies = 5; // Total number of enemies to spawn (excluding factions)
	public int numberOfFaction1 = 3; // Number of Faction 1 ships to spawn
	public int numberOfFaction2 = 3; // Number of Faction 2 ships to spawn
	public float spawnRadius = 20f;
	public float formationSpacing = 5f; // Spacing between ships in the formation
	public GameObject faction1LeaderPrefab; // Empty GameObject to act as a leader
	public GameObject faction2LeaderPrefab;
	private GameObject faction1Leader;
	private GameObject faction2Leader;

	void Start()
	{
		// Spawn normal enemy ships
		for (int i = 0; i < numberOfEnemies; i++)
		{
			SpawnEnemy();
		}

		// Get the player's position to spawn factions far from them
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		Vector3 playerPosition = player != null ? player.transform.position : Vector3.zero;

		// Ensure leaders spawn far away
		Vector3 faction1LeaderPosition = playerPosition + new Vector3(-50, 0, -50); // Move leader far away
		Vector3 faction2LeaderPosition = playerPosition + new Vector3(50, 0, 50); // Move leader far away

		// Spawn leaders at a distant position
		faction1Leader = Instantiate(faction1LeaderPrefab, faction1LeaderPosition, Quaternion.identity);
		faction2Leader = Instantiate(faction2LeaderPrefab, faction2LeaderPosition, Quaternion.identity);

		// Spawn faction ships with correct leader positions
		SpawnFaction1(faction1Leader.transform);
		SpawnFaction2(faction2Leader.transform);
	}


	void SpawnFaction1(Transform leader)
	{
		for (int i = 0; i < numberOfFaction1; i++)
		{
			Vector3 spawnPosition = leader.position + GetFormationOffset(i, numberOfFaction1) * formationSpacing;
			spawnPosition.y = 0; // Ensure ships stay on the same horizontal plane

			GameObject faction1Ship = Instantiate(faction1Prefab, spawnPosition, Quaternion.identity);

			// Assign formation leader to AI
			Faction1AI faction1AI = faction1Ship.GetComponent<Faction1AI>();
			if (faction1AI != null)
			{
				faction1AI.SetFormationLeader(leader);
				faction1AI.role = (Faction1AI.Role)Random.Range(0, 3);
			}
		}
	}

	void SpawnFaction2(Transform leader)
	{
		for (int i = 0; i < numberOfFaction2; i++)
		{
			Vector3 spawnPosition = leader.position + new Vector3(i * formationSpacing * 2f, 0f, 0f); // Increased spacing
			spawnPosition.y = 0; // Ensure ships stay on the same horizontal plane

			GameObject faction2Ship = Instantiate(faction2Prefab, spawnPosition, Quaternion.identity);

			Faction2AI faction2AI = faction2Ship.GetComponent<Faction2AI>();
			if (faction2AI != null)
			{
				faction2AI.SetFormationLeader(leader);
				faction2AI.role = (Faction2AI.Role)Random.Range(0, 3);
			}
		}
	}


	void SpawnEnemy()
	{
		if (enemyPrefabs.Length == 0)
		{
			Debug.LogError("No enemy prefabs assigned in the inspector!");
			return;
		}

		Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
		randomPosition += transform.position;
		randomPosition.y = 0; // Keep enemies on the same horizontal level

		GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
		Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
	}




	Vector3 GetFormationOffset(int index, int totalShips)
	{
		float randomOffset = Random.Range(-1f, 1f); // Small random variation

		if (totalShips == 3)
		{
			switch (index)
			{
				case 0: return new Vector3(0f + randomOffset, 0f, 0f); // Center
				case 1: return new Vector3(-1f + randomOffset, 0f, -1f); // Left
				case 2: return new Vector3(1f + randomOffset, 0f, -1f); // Right
			}
		}

		return new Vector3((index - (totalShips / 2)) + randomOffset, 0f, 0f);
	}

}
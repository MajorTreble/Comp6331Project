using UnityEngine;

public class AISpawner : MonoBehaviour
{
	public GameObject[] enemyPrefabs; // Array of enemy prefabs (Faction1, Faction2, Pirates, Solo)
	public int numberOfEnemies = 5; // Number of enemies to spawn
	public float spawnRadius = 20f; // Spawn radius

	void Start()
	{
		for (int i = 0; i < numberOfEnemies; i++)
		{
			SpawnEnemy();
		}
	}

	void SpawnEnemy()
	{
		// Random position within spawn radius
		Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
		randomPosition += transform.position;

		// Randomly select an enemy prefab
		GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
		GameObject enemy = Instantiate(enemyPrefab, randomPosition, Quaternion.identity);

		// Assign AIBehavior Scriptable Object
		EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
		if (enemyAI != null)
		{
			// Assign the corresponding AIBehavior Scriptable Object
			switch (enemyAI.behavior.faction)
			{
				case AIBehavior.Faction.Faction1:
					enemyAI.behavior = Resources.Load<AIBehavior>("ScriptableObjects/Faction1Behavior");
					break;
				case AIBehavior.Faction.Faction2:
					enemyAI.behavior = Resources.Load<AIBehavior>("ScriptableObjects/Faction2Behavior");
					break;
				case AIBehavior.Faction.Pirates:
					enemyAI.behavior = Resources.Load<AIBehavior>("ScriptableObjects/PiratesBehavior");
					break;
				case AIBehavior.Faction.Solo:
					enemyAI.behavior = Resources.Load<AIBehavior>("ScriptableObjects/SoloBehavior");
					break;
			}
		}
	}
}
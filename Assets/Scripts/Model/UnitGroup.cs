using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{

	[CreateAssetMenu(fileName = "UnitGroup", menuName = "ScriptableObjects/UnitGroup", order = 1)]
	public class UnitGroup : ScriptableObject
	{
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
		public float radius = 30.0f;
		public float minSpacing = 3.0f;

		public Faction faction = null;
		public List<ShipType> shipTypes = new List<ShipType>();
		private List<Vector3> usedPositions = new List<Vector3>();

		public Vector3 SpawnPosition()
		{
			//Vector3 randomPosition = Random.insideUnitSphere * radius;
			Vector3 randomPosition;
			int attempts = 0;
			const int maxAttempts = 50;
			do
			{
				// Get random position within radius
				randomPosition = Random.insideUnitSphere * radius;
				randomPosition.y = 0.0f;
				randomPosition += position;

				attempts++;
				if (attempts >= maxAttempts)
				{
					Debug.LogWarning("Couldn't find valid spawn position after " + maxAttempts + " attempts");
					break;
				}
			}
			// Keep trying until we find a position that's not too close to others
			while (IsTooCloseToOthers(randomPosition));

			usedPositions.Add(randomPosition);
			return randomPosition;
		}

		private bool IsTooCloseToOthers(Vector3 position)
		{
			foreach (Vector3 usedPos in usedPositions)
			{
				if (Vector3.Distance(position, usedPos) < minSpacing)
				{
					return true;
				}
			}
			return false;
		}

		public void ResetSpawnPositions()
		{
			usedPositions.Clear();
		}
	}

}
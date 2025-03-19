using UnityEngine;

[CreateAssetMenu(fileName = "New Reputation Data", menuName = "AI/Reputation")]
public class ReputationData : ScriptableObject
{
	public float faction1Reputation;
	public float faction2Reputation;
	public float piratesReputation;

	public float GetReputation(AIBehavior.Faction faction)
	{
		switch (faction)
		{
			case AIBehavior.Faction.Faction1:
				return faction1Reputation;
			case AIBehavior.Faction.Faction2:
				return faction2Reputation;
			case AIBehavior.Faction.Pirates:
				return piratesReputation;
			default:
				return 0f; // Default reputation for unknown factions
		}
	}

	public void AdjustReputation(AIBehavior.Faction faction, float amount)
	{
		switch (faction)
		{
			case AIBehavior.Faction.Faction1:
				faction1Reputation += amount;
				break;
			case AIBehavior.Faction.Faction2:
				faction2Reputation += amount;
				break;
			case AIBehavior.Faction.Pirates:
				piratesReputation += amount;
				break;
		}
	}
}
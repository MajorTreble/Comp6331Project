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
			case AIBehavior.Faction.Colonial:
				return faction1Reputation;
			case AIBehavior.Faction.Earth:
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
			case AIBehavior.Faction.Colonial:
				faction1Reputation += amount;
				break;
			case AIBehavior.Faction.Earth:
				faction2Reputation += amount;
				break;
			case AIBehavior.Faction.Pirates:
				piratesReputation += amount;
				break;
		}

		faction1Reputation = Mathf.Clamp(faction1Reputation, 0, 100);
		faction2Reputation = Mathf.Clamp(faction2Reputation, 0, 100);
		piratesReputation = Mathf.Clamp(piratesReputation, 0, 100);
	}
}
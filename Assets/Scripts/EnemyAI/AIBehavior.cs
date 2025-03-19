using UnityEngine;

[CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior")]
public class AIBehavior : ScriptableObject
{
	public enum Faction { Faction1, Faction2, Pirates, Solo }

	public Faction faction;
	public float aggressionLevel; // How aggressive this faction is
	public float reputationThreshold; // Reputation level required to avoid attacks
	public float attackRange;
	public float roamSpeed;
	public float chaseSpeed;
	public float rotationSpeed;

	// Add other behavior-specific parameters here
}
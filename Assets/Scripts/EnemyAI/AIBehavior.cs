// AIBehavior.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New AI Behavior", menuName = "AI/Behavior")]
public class AIBehavior : ScriptableObject
{
	public enum Faction { Faction1, Faction2, Pirates, Solo }

	[Header("General Settings")]
	public Faction faction;
	public float aggressionLevel; // How aggressive this faction is
	public float reputationThreshold; // Reputation level required to avoid attacks
	public float allyReputationThreshold = 70f; // Reputation level required to ally with the player
	public float allyAssistRange = 20f; // Range within which the AI will assist the player

	[Header("Movement")]
	public float roamSpeed;
	public float chaseSpeed;
	public float rotationSpeed;

	[Header("Combat")]
	public float attackRange;
	public float attackCooldown;
}
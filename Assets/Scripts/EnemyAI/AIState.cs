// AIState.cs
public enum AIState
{
	Roaming,    // The AI is wandering around
	Seeking,    // The AI is chasing or attacking the player
	Fleeing,     // The AI is running away (e.g., when health is low)
	AllyAssisting // The AI is assisting the player

}
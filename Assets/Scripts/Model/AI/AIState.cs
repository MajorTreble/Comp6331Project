// AIState.cs

namespace Model.AI
{
	public enum AIState
	{
		None,
		Idle,
		Roam,    // The AI is wandering around
		Combat,    // The AI is chasing or attacking the player
		Flee,     // The AI is running away (e.g., when health is low)
		AllyAssist, // The AI is assisting the player
		Patrol,
		Attack,
        Formation
	}
}
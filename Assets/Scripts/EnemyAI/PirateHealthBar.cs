using UnityEngine;
using UnityEngine.UI;

public class PirateHealthBar : MonoBehaviour
{
	public Slider healthSlider; // Reference to the UI Slider
	EnemyAI enemyAI; // Reference to the EnemyAI script

	void Update()
	{
		if (enemyAI != null && healthSlider != null)
		{
			// Update the health bar value
			healthSlider.value = enemyAI.health / enemyAI.maxHealth;
		}
	}
}
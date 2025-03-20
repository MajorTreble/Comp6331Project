using UnityEngine;
using UnityEngine.UI;

public class PirateHealthBar : MonoBehaviour
{
	public Slider healthSlider; // Reference to the UI Slider
	PirateShipScript pirateShip; // Reference to the PirateShipScript

	void Start()
	{
		// Find the PirateShipScript component on the same GameObject or a parent/child
		pirateShip = GetComponentInParent<PirateShipScript>();

		if (pirateShip == null)
		{
			Debug.LogError("PirateShipScript component not found on the pirate ship!");
		}

		if (healthSlider == null)
		{
			Debug.LogError("Health Slider is not assigned in the Inspector!");
		}
	}

	void Update()
	{
		if (pirateShip != null && healthSlider != null)
		{
			// Update the health bar value
			healthSlider.value = pirateShip.health / pirateShip.maxHealth;
		}
	}
}
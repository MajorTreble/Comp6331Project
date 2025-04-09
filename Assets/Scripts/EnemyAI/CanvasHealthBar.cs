using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class CanvasHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarSprite;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        healthBarSprite.fillAmount = currentHealth / maxHealth;
    }

	void Update()
	{
		if (cam == null) return;
		transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
	}

}

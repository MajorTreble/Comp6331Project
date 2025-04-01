using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Manager;
using Model;
using Controller;

public class ShipHUD : MonoBehaviour
{
	private Text ammoText;
	private Image healthImage;
	private Image shieldsImage;
	
	private Slider sld_Speed;

	PlayerShip ship;
	PlayerController pc;

	private void Awake()
	{
		GameObject ui = GameObject.Find("Canvas");
		ammoText = ui.transform.Find("Ammo").transform.Find("Text").GetComponent<Text>();
		healthImage = ui.transform.Find("Health Bar").GetComponent<Image>();
		shieldsImage = ui.transform.Find("Shields").GetComponent<Image>();		
		sld_Speed = ui.transform.Find("Sld_Speed").GetComponent<Slider>();

		
	}

	private void Start() 
	{
		if (GameManager.Instance.playerShip != null)
		{
			ship = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
			pc = GameManager.Instance.playerShip.GetComponent<PlayerController>();
		}
	}

	private void Update()
	{
		if (GameManager.Instance.playerShip == null) return;
		if (ship == null || pc == null)
		{
			ship = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
			pc = GameManager.Instance.playerShip.GetComponent<PlayerController>();
			Debug.Log("SHIP OR PC NULL");
			return;
		}

		
		if(ship!=null)
		{
			ammoText.text = ship.ammo.ToString();
			healthImage.fillAmount = ship.health / ship.CurrMaxHealth;
			shieldsImage.fillAmount = ship.shields / ship.CurrMaxShields;
			sld_Speed.value = pc.currSpeed/ship.CurrMaxSpeed;
		}
	}

	public void Portal()
	{
		ship.Leave();
		//GameManager.Instance.StopScenario();
	}
}

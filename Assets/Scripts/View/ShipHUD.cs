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

	private GameObject tutorialButton;

	PlayerShip ship;
	PlayerController pc;

	private Image playerAim;

	private void Awake()
	{
		GameObject ui = GameObject.Find("Canvas");
		ammoText = ui.transform.Find("Ammo").transform.Find("Text").GetComponent<Text>();
		healthImage = ui.transform.Find("Health Bar").GetComponent<Image>();
		shieldsImage = ui.transform.Find("Shields").GetComponent<Image>();		
		sld_Speed = ui.transform.Find("Sld_Speed").GetComponent<Slider>();
			
		playerAim = ui.transform.Find("Aim").GetChild(0).GetComponent<Image>();

		tutorialButton = ui.transform.Find("Tutorial").gameObject;
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
			if(ship.ammo < 1) JobView.Inst.UpdateJob();

			ammoText.text = ship.ammo.ToString();
			healthImage.fillAmount = ship.health / ship.CurrMaxHealth;
			shieldsImage.fillAmount = ship.shields / ship.CurrMaxShields;
			sld_Speed.value = Mathf.Abs(pc.currSpeed)/ship.CurrMaxSpeed;

			if(pc.currSpeed > 0)
			{
				Utils.FindChildByName(sld_Speed.transform, "Fill").GetComponent<Image>().color = Color.green;
				sld_Speed.direction = Slider.Direction.BottomToTop;
			}else
			{
				Utils.FindChildByName(sld_Speed.transform, "Fill").GetComponent<Image>().color = Color.red;
				sld_Speed.direction = Slider.Direction.TopToBottom;
			} 

			UpdateAim();
		}
	}

	public void Portal()
	{
		ship.Leave();
		//GameManager.Instance.StopScenario();
	}

	public void AcceptTutorial()
	{
		tutorialButton.SetActive(false);
		Time.timeScale = 1;
	}

	void UpdateAim()
	{
		Vector3 aimPos = playerAim.transform.position;
		Vector3 mousePos = Input.mousePosition;

		playerAim.transform.position = mousePos;


	}
}

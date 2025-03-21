using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Manager;
using Model;

public class ShipHUD : MonoBehaviour
{
	private Text jobNameText;
	private Text ammoText;
	private Image healthImage;
	private Image shieldsImage;

	private void Awake()
	{
		GameObject ui = GameObject.Find("Canvas");
		jobNameText = Utils.FindChildByName(ui.transform, "JobFeedbackText").GetComponent<Text>();
		ammoText = ui.transform.Find("Ammo").transform.Find("Text").GetComponent<Text>();
		healthImage = ui.transform.Find("Health Bar").GetComponent<Image>();
		shieldsImage = ui.transform.Find("Shields").GetComponent<Image>();

		Debug.LogWarning("There is some kind of error on update/ship, commented for now");
	}

	private void Update()
	{
		//JobModel job = JobMenuController.Inst.currJob;
		//jobNameText.text = job.jobName;

		PlayerShip ship = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
		if(ship!=null)
		{
			ammoText.text = ship.ammo.ToString();
			healthImage.fillAmount = ship.health / ship.maxHealth;
			shieldsImage.fillAmount = ship.shields / ship.maxShields;
		}
	}

	public void Portal()
	{
		GameManager.Instance.StopScenario();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Manager;

namespace Controller
{

	public class MainMenuController : MonoBehaviour
	{
		public Vector3 playerShipPosition;
		public Quaternion playerShipQuaternion;

		public void Start()
		{
			GameManager.Instance.SpawnPlayer(playerShipPosition, playerShipQuaternion);
		}

		public void NewGame()
		{
			GameManager.Instance.NewGame();
		}

		public void LoadGame()
		{
			GameManager.Instance.LoadGame();
		}

		public void QuitGame()
		{
			Application.Quit();
		}

		public void Job()
		{
			SceneManager.LoadScene(1);
		}
	}

}
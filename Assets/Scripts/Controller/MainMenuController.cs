using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Manager;

namespace Controller
{

	public class MainMenuController : MonoBehaviour
	{

		public void NewGame()
		{
			GameManager.Instance.NewGame();
		}

		public void Job()
		{
			SceneManager.LoadScene(1);
		}
	}

}
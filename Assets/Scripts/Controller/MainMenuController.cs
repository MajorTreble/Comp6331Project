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

		public void Play()
		{
			GameManager.Instance.Play();
		}

		public void Load()
		{
			GameManager.Instance.Load();
		}

		public void Quit()
		{

			#if UNITY_EDITOR
        	UnityEditor.EditorApplication.isPlaying = false;
        	#else
        	Application.Quit();
        	#endif

		}

		public void QuickPlay()
		{
			GameManager.Instance.QuickPlay();
		}
	}

}
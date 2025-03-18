using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameObject playerPrefab;
        public GameObject playerShip;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

		private void Start()
		{
        }

        public void NewGame()
		{
            PersistenceManager.Instance.NewGame();

            SceneManager.LoadScene(2);
        }

        public void LoadGame()
        {
            PersistenceManager.Instance.LoadGame();

            SceneManager.LoadScene(2);
        }

        public void SaveGame()
        {
            PersistenceManager.Instance.SaveGame();
        }

        public void SpawnPlayer(Vector3 spawnPosition, Quaternion spawnQuaternion)
        {
            playerShip = GameObject.Instantiate(playerPrefab, spawnPosition, spawnQuaternion);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!JobController.Inst)
			{
                return;
			}

            JobController.JobStatus jobStatus = JobController.Inst.jobStatus;
            if (jobStatus == JobController.JobStatus.NotSelected)
			{
                JobController.Inst.AcceptJob(1);
            }

            SpawnPlayer(Vector3.zero, Quaternion.identity);
        }
    }

}
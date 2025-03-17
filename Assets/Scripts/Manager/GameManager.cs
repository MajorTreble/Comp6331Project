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
            GameObject.Instantiate(playerPrefab, spawnPosition, spawnQuaternion);
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

            JobModel job = JobController.Inst.currJob;

            GameObject ui = GameObject.Find("Canvas");
            ui.transform.Find("Job Name").GetComponent<Text>().text = job.jobName;
        }
    }

}
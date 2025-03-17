using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

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
            SceneManager.LoadScene(2);
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
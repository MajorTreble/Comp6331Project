using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEditor;
using Model;

//[CustomEditor(typeof(Job))]
public class JobEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Review Jobs"))
        {
            ChangeJobs();
        }
    }

    private void ChangeJobs()
    {
        string path = "Assets/Resources/Scriptable/Jobs"; 
        string[] guids = AssetDatabase.FindAssets("t:Job", new[] { path });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Job job = AssetDatabase.LoadAssetAtPath<Job>(assetPath);

            if (job != null)
            {
                job.jobName = $"{job.jobType} {job.rewardType}";
                job.jobDescription = $"{job.jobType} {job.quantity} {job.jobTarget}";

                EditorUtility.SetDirty(job);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}

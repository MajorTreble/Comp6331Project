using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
	public enum ScenarioDifficulty {  Easy, Medium, Hard }

	[CreateAssetMenu(fileName = "Scenario", menuName = "ScriptableObjects/Scenario", order = 1)]
	public class Scenario : ScriptableObject
	{
		public string mapName = "PlaceHolder Name";
		public string sceneName = "SkyBoxScene";

		public Vector3 playerPosition = Vector3.zero;
		public Quaternion playerRotation = Quaternion.identity;

		public Vector3 portalPosition = Vector3.zero;

		public ScenarioDifficulty difficulty = ScenarioDifficulty.Easy;
		public List<JobType> supportedJobType = new List<JobType>();

		public List<UnitGroup> unitGroups = new List<UnitGroup>();

		public List<SpaceObjectGroup> spaceObjectGroups = new List<SpaceObjectGroup>();
	}

}
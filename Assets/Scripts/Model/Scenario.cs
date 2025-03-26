using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
	[CreateAssetMenu(fileName = "Scenario", menuName = "ScriptableObjects/Scenario", order = 1)]
	public class Scenario : ScriptableObject
	{
		public string mapName = "PlaceHolder Name";
		public string sceneName = "SkyBoxScene";
		public Vector3 playerPosition = Vector3.zero;
		public Quaternion playerRotation = Quaternion.identity;
	}

}
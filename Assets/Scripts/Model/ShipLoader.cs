using UnityEngine;

namespace Model
{

    [CreateAssetMenu(fileName = "New Ship Loader", menuName = "Ship Loader")]
    public class ShipLoader : ScriptableObject
    {
        public GameObject playerShipPrefab;
        public GameObject aiShipPrefab;
        public GameObject breakableAsteroidPrefab;
        public GameObject mineableAsteroidPrefab;
        public GameObject magneticAsteroidPrefab;
    }

}
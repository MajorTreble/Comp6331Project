using UnityEngine;


namespace Model.Environment
{
    public class MineableAsteroid : Asteroid
    {
        [Header("Spinning")]
        public float spinSpeed = 10f;

        void Update()
        {
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
        }
    }
}

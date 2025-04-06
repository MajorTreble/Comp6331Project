using UnityEngine;

namespace Model.Environment
{
    public class MineableAsteroid : MonoBehaviour
    {
        [Header("Spinning")]
        public float spinSpeed = 10f;

        void Update()
        {
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayerLaser") || other.CompareTag("SpaceshipComponent") || other.CompareTag("Player"))
            {
                Debug.Log("[MineableAsteroid] Hit by mining tool!");

                Destroy(gameObject);
            }
        }

    }
}

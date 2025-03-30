using UnityEngine;

namespace Model.Environment
{
    public class CameraShaker : MonoBehaviour
    {
        public float shakeAmount = 0.2f;
        public float shakeSpeed = 20f;
        public bool isShaking = false;

        private Vector3 initialPosition;

        void Start()
        {
            initialPosition = transform.localPosition;
        }

        void Update()
        {
            if (isShaking)
            {
                Vector3 offset = Random.insideUnitSphere * shakeAmount;
                transform.localPosition = initialPosition + offset;
            }
        }

        public void StartShake()
        {
            if (!isShaking)
            {
                initialPosition = transform.localPosition;
                isShaking = true;
            }
        }

        public void StopShake()
        {
            isShaking = false;
            transform.localPosition = initialPosition;
        }
    }

}
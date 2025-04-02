using UnityEngine;

namespace Model.Environment
{
    public class BlackHoleSpawnEffect : MonoBehaviour
    {
        public float growDuration = 2f;
        public float maxScale = 0.01f;
        public float spinSpeed = 90f;

        private float timer = 0f;

        void Start()
        {
            transform.localScale = Vector3.zero;
            StartCoroutine(GrowAndSpin());
        }

        void Update()
        {
            // Always spin
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);
        }

        private System.Collections.IEnumerator GrowAndSpin()
        {
            while (timer < growDuration)
            {
                float t = timer / growDuration;
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * maxScale, t);
                timer += Time.deltaTime;
                yield return null;
            }

            transform.localScale = Vector3.one * maxScale;
        }
    }
}

using Manager;
using UnityEngine;

namespace Controller {
    public class StarfieldController : MonoBehaviour
    {
        public Rigidbody playerRb;
        public ParticleSystem starfield;

        public float baseSpeed = -10f; 
        public float speedMultiplier = -1.5f;

        public float baseEmission = 100f;
        public float emissionMultiplier = 5f;

        void Start()
        {
            if (starfield == null)
                starfield = GetComponent<ParticleSystem>();

            if (playerRb == null && GameManager.Instance?.playerShip != null)
                playerRb = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
        }

        void Update()
        {
            if (playerRb == null || starfield == null) return;

            float playerSpeed = playerRb.velocity.magnitude;

            var main = starfield.main;
            var emission = starfield.emission;

            if (playerSpeed < 0.1f)
            {
                main.startSpeed = 0f;
                emission.rateOverTime = 0f;
            }
            else
            {
                main.startSpeed = baseSpeed + (playerSpeed * speedMultiplier);
                float rate = baseEmission + (playerSpeed * emissionMultiplier);
                emission.rateOverTime = rate;
                main.maxParticles = (int)(rate * 10);
            }
        }
    }

}
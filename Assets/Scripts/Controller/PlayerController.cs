using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Manager;
using Model;

namespace Controller
{

    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float turnSpeed = 100f;
        public float mouseSensitivity = 2f;

        private float verticalInput;
        private float horizontalInput;
        private float mouseX;
        private float mouseY;

        Vector3 Velocity;
        float maxSpeed = 5.0f;

        void Update()
        {
            GameObject playerShip = GameManager.Instance.playerShip;
            if (playerShip == null)
            {
                return;
            }

            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            Vector3 desiredMovement = Vector3.forward * verticalInput * moveSpeed * Time.deltaTime;
            desiredMovement.y = Mathf.Clamp(desiredMovement.y, 0, 2);
            transform.Translate(desiredMovement);
            transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            transform.Rotate(Vector3.up * mouseX * mouseSensitivity);
            transform.Rotate(Vector3.left * mouseY * mouseSensitivity);

            playerShip.GetComponent<PlayerShip>().ShowLaser(Input.GetKey(KeyCode.Space));

            Vector3 direction = playerShip.transform.forward * verticalInput;
            Velocity = direction.normalized * maxSpeed;
            playerShip.transform.position += Velocity * Time.deltaTime;

            float angle = 0.5f * horizontalInput;
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                playerShip.transform.rotation = Quaternion.LookRotation(
                    Quaternion.AngleAxis(angle, Vector3.up) * playerShip.transform.forward);
            }
        }

    }

}
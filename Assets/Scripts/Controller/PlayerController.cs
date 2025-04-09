using UnityEngine;

using Manager;
using Model;
using UnityEngine.UI;

namespace Controller
{

    public class PlayerController : MonoBehaviour
    {

        public float currSpeed;
        public int throttle;
        public float mouseRotFac ;


        Vector3 Velocity;
        Rigidbody playerShipRb;
        PlayerShip playerShip;

        bool devTools = false;
        bool rotOnMouse = false;

        void Start()
        {
            if (GameManager.Instance != null)
            {
                playerShipRb = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
                playerShip = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
            }

            mouseRotFac = mouseRotFac == 0 ? 0.05f : mouseRotFac;
        }

        void DevTools()
        {
            if(Input.GetKey(KeyCode.F1)) playerShip.Leave();
        }

        void LateUpdate()
        {
            if(GameManager.Instance != null && GameManager.devTools) DevTools();

            if (GameManager.Instance != null && GameManager.Instance.onMenu)
                return;

            if (GameManager.Instance == null)
            {
                return;
            }

            if (playerShipRb == null || playerShip == null)
            {
                playerShipRb = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
                playerShip = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
                return;
            }

            if (Input.GetKey(KeyCode.Z)) Stabilize();
            if (Input.GetKey(KeyCode.G)) GiveUpJob();

            ShipRotation();
            ShipMovement();
            Shoot();


            if (CameraFollow.Inst != null)
            {
                CameraFollow.Inst.MoveCamera();
            }
        }

        void ShipRotation()
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");            
            Vector3 rotation = new Vector3(-vertical, horizontal, 0) * playerShip.CurrTurnSpeed * Time.deltaTime;
            Quaternion finalRot = playerShipRb.rotation * Quaternion.Euler(rotation);
            
            if(Input.GetKey(KeyCode.M)) rotOnMouse = !rotOnMouse;
            if(rotOnMouse)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                Vector3 mouseOffset = mousePos - screenCenter;
                Vector3 mouseRot = new Vector3(-mouseOffset.y, mouseOffset.x, 0) * mouseRotFac * Time.deltaTime;
                Vector3 combRot = rotation + mouseRot;
                Quaternion deltaRot = Quaternion.Euler(combRot);
                finalRot = playerShipRb.rotation * deltaRot;
            }

            playerShipRb.MoveRotation(finalRot);
        }

        void ShipMovement()
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1))
                throttle += 1;
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Alpha1))
                throttle -= 1;

            throttle = Mathf.Clamp(throttle, -1, 3);
            float desiredMov = playerShip.CurrMaxSpeed * throttle / 3;

            currSpeed = Mathf.Lerp(currSpeed, desiredMov, playerShip.CurrAcc * Time.deltaTime);
            
            playerShipRb.MovePosition(playerShipRb.position + (playerShipRb.transform.forward * currSpeed * Time.deltaTime));

            
        }

        void Shoot()
        {
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0) )
            {
                if(playerShip.ammo > 0)
                    playerShipRb.GetComponent<PlayerShip>().Attack();
            }
        }


        void Stabilize()
        {
            currSpeed = currSpeed = Mathf.Clamp(currSpeed + (-playerShip.CurrAcc * Time.deltaTime), 0, playerShip.CurrMaxSpeed);

            playerShipRb.velocity = Vector3.Lerp(playerShipRb.velocity, Vector3.zero, Time.deltaTime);
            playerShipRb.angularVelocity = Vector3.Lerp(playerShipRb.angularVelocity, Vector3.zero, Time.deltaTime);
            playerShipRb.rotation = Quaternion.Lerp(playerShipRb.rotation, Quaternion.identity, Time.deltaTime);
        }

        void GiveUpJob()
        {
            JobController.Inst.FailJob();
        }
    }

}
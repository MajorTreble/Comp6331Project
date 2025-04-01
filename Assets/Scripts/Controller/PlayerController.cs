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
        public float mouseSensitivity;


        Vector3 Velocity;
        Rigidbody playerShipRb;
        PlayerShip playerShip;

        void Start()
        {
            playerShipRb = GameManager.Instance.playerShip.GetComponent<Rigidbody>();  
            playerShip = GameManager.Instance.playerShip.GetComponent<PlayerShip>();  

            mouseSensitivity = mouseSensitivity == 0 ? 10 : mouseSensitivity;
        }

        void LateUpdate()
        {            
            if(GameManager.Instance.onMenu)
                return;

            if (playerShipRb == null || playerShip == null)
            {
                playerShipRb = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
                playerShip = GameManager.Instance.playerShip.GetComponent<PlayerShip>();  
                return;
            }

            //Input 
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                throttle += 1;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                throttle -= 1;

            throttle = Mathf.Clamp(throttle, -1,3);
            float desiredMov = playerShip.CurrMaxSpeed * throttle/3;
            //Mov
            currSpeed = Mathf.Lerp(currSpeed, desiredMov, playerShip.CurrAcc * Time.deltaTime);

            //Rot
            Vector3 rotation = new Vector3(-mouseY, mouseX, horizontal) *  playerShip.CurrTurnSpeed * Time.deltaTime * mouseSensitivity;
        
            //RigidBody based movement
            playerShipRb.MovePosition(playerShipRb.position + (playerShipRb.transform.forward * currSpeed * Time.deltaTime));
            Quaternion deltaRotation = Quaternion.Euler(rotation);
            playerShipRb.MoveRotation(playerShipRb.rotation * deltaRotation);    

            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
            {
                playerShipRb.GetComponent<PlayerShip>().FireLaser();
            }

            if (Input.GetKey(KeyCode.E)) Stabilize();

            CameraFollow.Inst.MoveCamera();
        }


        void Stabilize()
        {
            currSpeed = currSpeed = Mathf.Clamp(currSpeed + (- playerShip.CurrAcc * Time.deltaTime), 0, playerShip.CurrMaxSpeed);

            playerShipRb.velocity = Vector3.Lerp(playerShipRb.velocity, Vector3.zero, Time.deltaTime);
            playerShipRb.angularVelocity = Vector3.Lerp(playerShipRb.angularVelocity, Vector3.zero, Time.deltaTime);            
            playerShipRb.rotation = Quaternion.Lerp(playerShipRb.rotation, Quaternion.identity, Time.deltaTime);
        }
    }      
  
}
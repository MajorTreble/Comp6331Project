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

        bool isSpacePressed = false;

        Vector3 Velocity;
        float maxSpeed = 5.0f;

        void Update()
        {
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



            GameObject playerShip = GameManager.Instance.playerShip;

            playerShip.GetComponent<PlayerShip>().ShowLaser(Input.GetKey(KeyCode.Space));

            Vector3 direction = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
            {
                direction += playerShip.transform.forward;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                direction += -playerShip.transform.forward;
            }

            Velocity = direction.normalized * maxSpeed;

            playerShip.transform.position += Velocity * Time.deltaTime;

            float angle = 0.5f;
            if (Input.GetKey(KeyCode.A))
            {
                playerShip.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(-angle, Vector3.up) * playerShip.transform.forward);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                playerShip.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle, Vector3.up) * playerShip.transform.forward);
            }
        }



        public List<Reputation> reputations = new List<Reputation>();
        public int coins;

        public static PlayerController Inst { get; private set; } //Singleton
        private void Awake()
        {
            if (Inst == null)
            {
                Inst = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        
        public void Start()
        {
            Debug.LogWarning("player and camera scripts are deactivated for testing");
            

            reputations.Add(new Reputation (RepType.Faction1, 0));
            reputations.Add(new Reputation (RepType.Faction2, 0));
            reputations.Add(new Reputation (RepType.Pirate, 0));
            reputations.Add(new Reputation (RepType.Self, 0));     
        }



        public void ChangeReputation(RepType _type, int _value)
        {
            Reputation rep = reputations.Find(i => i.type == _type);
            if (rep != null)
                rep.ChangeValue(_value);
            else
                Debug.LogWarning($"Item of type {_type} not found.");
            
        }
    }     
    
}
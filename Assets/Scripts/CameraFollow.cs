using UnityEngine;
using Manager;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("View Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -10);
    public Vector3 cockpitOffset = new Vector3(0, 1, 1);

    [Header("Cockpit View")]
    public bool cockpitView = false;

    public float smoothSpeed = 0.125f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (GameManager.Instance.playerShip != null)
        {
            player = GameManager.Instance.playerShip.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cockpitView = !cockpitView;
        }
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 offset = cockpitView ? cockpitOffset : thirdPersonOffset;
        Vector3 desiredPosition = player.position + player.rotation * offset;
        Quaternion desiredRotation = Quaternion.LookRotation(player.forward, Vector3.up);

        if (cockpitView)
        {
            // Stick to cockpit immediately (no smoothing)
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
        else
        {
            // Smooth follow for third-person view
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
        }
    }
}

using UnityEngine;

public class IceAsteroid : MonoBehaviour
{
    public IceBlurEffect blurEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerComponent"))
        {
            Debug.Log("Player entered the ice asteroid! Applying blur effect...");
            blurEffect.ApplyBlur(4f); // Apply blur for 4 seconds
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerComponent"))
        {
            Debug.Log("Player exited the ice asteroid! Removing blur effect...");
            blurEffect.RemoveBlur();
        }
    }
}

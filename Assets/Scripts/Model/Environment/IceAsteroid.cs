using UnityEngine;

namespace Model.Environment {
    public class IceAsteroid : MonoBehaviour
    {
        private IceBlurEffect blurEffect;

        void Start()
        {
            // IceBlurEffect script attached to the "BlurEffect" GameObject
            GameObject blurObject = GameObject.Find("BlurEffect");

            if (blurObject != null)
            {
                blurEffect = blurObject.GetComponent<IceBlurEffect>();

                if (blurEffect == null)
                    Debug.LogError("IceBlurEffect script not found on 'BlurEffect' GameObject!");
            }
            else
            {
                Debug.LogError("BlurEffect GameObject not found in the scene!");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerComponent"))
            {
                Debug.Log("Player entered the ice asteroid! Applying blur effect...");
                if (blurEffect != null)
                {
                    blurEffect.ApplyBlur(4f); // Apply blur for 4 seconds
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerComponent"))
            {
                Debug.Log("Player exited the ice asteroid! Removing blur effect...");
                if (blurEffect != null)
                {
                    blurEffect.RemoveBlur();
                }
            }
        }
    }

}
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

namespace Model.Environment {
    public class IceBlurEffect : MonoBehaviour
    {
        public PostProcessVolume postProcessVolume;
        private DepthOfField blurEffect;
        private bool isBlurring = false;


        void Start()
        {
            if (postProcessVolume.profile.TryGetSettings(out blurEffect))
            {
                //Debug.Log("Depth of Field effect found! Ready to apply blur.");
            }
            else
            {
                Debug.LogError("Depth of Field effect NOT found! Make sure it is added to the Post-Processing Profile.");
            }
        }

        public void ApplyBlur(float duration)
        {
            if (!isBlurring)
            {
                StartCoroutine(BlurEffectRoutine(duration));
            }
        }

        public void RemoveBlur()
        {
            if (isBlurring)
            {
                StartCoroutine(BlurFadeOut());
            }
        }

        private IEnumerator BlurEffectRoutine(float duration)
        {
            isBlurring = true;

            float elapsedTime = 0f;
            float startAperture = blurEffect.aperture.value;
            float targetBlur = 0.1f; // Max blur

            while (elapsedTime < duration / 2)
            {
                blurEffect.aperture.value = Mathf.Lerp(startAperture, targetBlur, elapsedTime / (duration / 2));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            blurEffect.aperture.value = targetBlur;
            yield return new WaitForSeconds(2f);
        }

        private IEnumerator BlurFadeOut()
        {
            float elapsedTime = 0f;
            float startBlur = blurEffect.aperture.value;
            float targetClear = 32f; // Fully clear

            while (elapsedTime < 5f)
            {
                blurEffect.aperture.value = Mathf.Lerp(startBlur, targetClear, elapsedTime / 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            blurEffect.aperture.value = targetClear;
            isBlurring = false;
        }
    }

}

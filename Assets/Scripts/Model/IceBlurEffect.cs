using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class IceBlurEffect : MonoBehaviour
{
    public PostProcessVolume postProcessVolume;
    private DepthOfField blurEffect;
    private bool isBlurring = false;

    void Start()
    {
        if (postProcessVolume.profile.TryGetSettings(out blurEffect))
        {
            Debug.Log("Depth of Field effect found! Ready to apply blur.");
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
        float maxBlur = 1.0f;

        while (elapsedTime < duration / 2)
        {
            blurEffect.aperture.value = Mathf.Lerp(startAperture, maxBlur, elapsedTime / (duration / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator BlurFadeOut()
    {
        float elapsedTime = 0f;
        float currentBlur = blurEffect.aperture.value;
        float minBlur = 0.1f;

        while (elapsedTime < 2f)
        {
            blurEffect.aperture.value = Mathf.Lerp(currentBlur, minBlur, elapsedTime / 2f);
            elapsedTime += Time.deltaTime; 
            yield return null;
        }

        blurEffect.aperture.value = 32;
        isBlurring = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using Controller;
using Manager;
using UnityEngine;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public static TargetController Inst { get; private set; } //Singleton

    Vector2 targetOriSize;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else Destroy(gameObject);
    }




    public Image targetIcon; 
    public Vector3 target;

    void Start()
    {
        targetOriSize = targetIcon.rectTransform.sizeDelta;
    }

    void Update()
    {
        target = SpawningManager.Instance.portalPosition;

        SetTarget(target, targetIcon);
        
    }

    void SetTarget(Vector3 _pos, Image _Icon)
    {
        if (target != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target);

            if (screenPos.z > 0)
            {
                screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
                screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
                targetIcon.transform.position = screenPos;
                targetIcon.enabled = true;

                float dist = Vector3.Distance(Camera.main.transform.position, target);

                float scale = Mathf.Clamp(100 / dist, 0.25f, 1f); 
                Vector2 newSize = Vector2.Lerp(targetOriSize*0.25f, targetOriSize, scale);

                targetIcon.rectTransform.sizeDelta = newSize;
            }
            else
            {
                targetIcon.enabled = false;
            }
        }

    }
}

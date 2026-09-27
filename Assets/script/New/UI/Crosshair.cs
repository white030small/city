using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 若在 Canvas UI 下直接套用螢幕座標
        if (rectTransform != null)
        {
            transform.position = Input.mousePosition;
        }
        else
        {
            if (Camera.main != null)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;
                transform.position = mousePos;
            }
        }
    }
}
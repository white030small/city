using UnityEngine;

public class camermover : MonoBehaviour
{
    [Header("跟隨目標")]
    public Transform targetReality;
    public Transform targetSpirit;

    [Header("設定")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    // 當前跟隨的目標
    private Transform currentTarget;

    void Start()
    {
        // 預設跟隨現實
        currentTarget = targetReality;
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 targetPosition = currentTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    // 切換跟隨目標
    public void SwitchTarget(bool toSpirit)
    {
        currentTarget = toSpirit ? targetSpirit : targetReality;
    }
}
using UnityEngine;

public class camermover : MonoBehaviour
{
    [Header("跟隨目標")]
    public Transform targetReality;
    public Transform targetSpirit;

    [Header("設定")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Transform currentTarget;
    //private bool isJumping = false;
    private float lockedY; // 跳躍時鎖住的 Y 座標

    void Start()
    {
        currentTarget = targetReality;
        lockedY = transform.position.y;
    }

    void LateUpdate()
    {
        if (currentTarget == null) return;

        Vector3 targetPosition = currentTarget.position + offset;

        //if (isJumping)
        //{
            // X 跟隨，Y 鎖住不動
            targetPosition.y = lockedY;
        //}

        transform.position = Vector3.Lerp(transform.position,targetPosition,smoothSpeed * Time.deltaTime);
    }

    public void SwitchTarget(bool toSpirit)
    {
        if (toSpirit)
        {
            currentTarget = targetSpirit;
        }
        else
        {
            currentTarget = targetReality;
        }

        // 切換世界時，更新鎖定的 Y 並解除跳躍狀態
        //isJumping = false;
        //lockedY = currentTarget.position.y + offset.y;
    }

    /*public void IsJump(bool jumping)
    {
        isJumping = jumping;
        if (!jumping)
        {
            // 落地時更新鎖定的 Y
            lockedY = currentTarget.position.y + offset.y;
        }
    }*/
}
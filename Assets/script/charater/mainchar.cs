using UnityEngine;

public class mainchar : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.5f;

    [Header("同步目標")]
    public Transform playerReality;   // 現實層的方塊
    public Transform playerSpirit;    // 靈界層的方塊

    private Vector2 moveInput;//座標
    private bool isRunning;

    void Update()
    {
        // 讀取輸入
        moveInput.x = Input.GetAxisRaw("Horizontal");  // A/D
        moveInput.y = Input.GetAxisRaw("Vertical");    // W/S
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // 正規化對角線移動
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // 計算移動
        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * currentSpeed * Time.deltaTime;
        Vector3 move_sprit = new Vector3(moveInput.x , -moveInput.y, 0)* currentSpeed * Time.deltaTime;

        // 同步移動兩個方塊
        if (playerReality != null)
        {
            playerReality.position += move;
        }

        if (playerSpirit != null)
        {
            playerSpirit.position += move_sprit;
        }
    }
}

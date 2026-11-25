using UnityEngine;

public class cameramover_sprit : MonoBehaviour
{
    public Transform target; // 要跟隨的角色
    public Vector3 offset = new Vector3(0, 0, -10);

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}

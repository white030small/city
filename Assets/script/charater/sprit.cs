using UnityEngine;

public class sprit : MonoBehaviour
{
    public mainchar mainchar;

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnCollisionEnter2D(Collision2D collision)//碰地板就回歸
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            mainchar.GetSpritJump(0);
        }
    }
}

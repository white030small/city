using UnityEngine;

public class InteractableGrab : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private bool isBeingDragged = false;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Collider2D objCollider; // ���󥻨����I����
    private mainchar playerScript; // �x�s�D���}�����Ѧ�

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        objCollider = GetComponent<Collider2D>();

        // ��l�ƴN�ꦺ X �b�P����A�T�O���`�����ʡA��������O����
        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(true);
    }

    void Update()
    {
        if (isBeingDragged && playerScript != null && !playerScript.isGrounded)
        {
            StopDragging();
        }

        // �u���� E �~�|�������A
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (!isBeingDragged)
            {
                // �i�����ˬd�j�u���D���b�a�W�ɡA�~��}�l�h�B
                if (playerScript != null && playerScript.isGrounded)
                {
                    StartDragging();
                }
            }
            else
            {
                StopDragging();
            }
        }
    }

    void StartDragging()
    {
        isBeingDragged = true;
        transform.SetParent(playerTransform);
        // �i�D�D���G�A�}�l�h�F��F
        if (playerScript != null) playerScript.isDraggingObject = true;
        // ����ɧאּ Kinematic �קK���z�ݰ�
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // ����ɼȮɩ����P���a���I���A������D�ɤ��۱���
        Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(objCollider, playerCollider, true);
        }
    }

    void StopDragging()
    {
        isBeingDragged = false;
        // �i�D�D���G�A���F
        if (playerScript != null) playerScript.isDraggingObject = false;
        // ��_�I������
        if (playerTransform != null)
        {
            Collider2D playerCollider = playerTransform.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(objCollider, playerCollider, false);
            }
        }

        transform.SetParent(null);
        rb.bodyType = RigidbodyType2D.Dynamic;
        LockPosition(true);
    }

    void LockPosition(bool locked)
    {
        if (locked)
        {
            // ��w X �P ����A�O�d Y �b������U�Y
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerTransform = other.transform;
            playerScript = other.GetComponent<mainchar>();
            
            // 靈體身上沒有 mainchar，去 main char 找
            if (playerScript == null)
            {
                playerScript = FindFirstObjectByType<mainchar>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if(!isBeingDragged)
            {
                playerTransform = null;
                playerScript = null;
            }
        }
    }
}
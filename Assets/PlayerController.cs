using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool isGhostMode = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ��l�]�w�G�H���Ҧ�
        UpdatePhysics();
    }

    void Update()
    {
        // 2D ���ʿ�J (���k: A/D, �W�U: W/S)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(moveX, moveY).normalized;

        // �]�w�t�� (Unity 6 ��ĳ�ϥ� linearVelocity)
        rb.linearVelocity = movement * moveSpeed;

        // ���ť���������F�Ҧ�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isGhostMode = !isGhostMode;
            UpdatePhysics();
        }
    }

    void UpdatePhysics()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int humanLayer = LayerMask.NameToLayer("HumanWorld");

        // 2D ����֤��޿�
        // �� isGhostMode �� true �ɡA���� Player �P HumanWorld ���I��
        Physics2D.IgnoreLayerCollision(playerLayer, humanLayer, isGhostMode);

        // ��ı�^�X�G���F�Ҧ��ܥb�z���Ŧ�
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = isGhostMode ? new Color(0, 1, 1, 0.5f) : Color.white;
    }
}
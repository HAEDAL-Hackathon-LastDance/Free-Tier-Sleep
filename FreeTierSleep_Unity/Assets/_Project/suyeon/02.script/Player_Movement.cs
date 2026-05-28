using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 1.5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ������ ���۵� �� Rigidbody2D ������Ʈ�� �� ���� �����ͼ� ������
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // �� �����Ӹ��� WASD �Է��� ������
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    // ���� ������ Update�� �ƴ� FixedUpdate���� ó���ϴ� ���� ���� ��Ģ�̾�
    void FixedUpdate()
    {
        // �밢�� �̵� �� �ӵ��� �������� ���� ���� ���� normalized�� ������
        rb.linearVelocity = movement.normalized * speed;
    }
}
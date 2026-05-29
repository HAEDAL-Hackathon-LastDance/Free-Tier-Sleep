using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2.5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    // Start is called before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 게임이 시작될 때 Rigidbody2D 컴포넌트를 이 변수에 할당합니다.
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // 매 프레임마다 WASD 입력을 받습니다.
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    // 물리 기반 이동은 Update가 아닌 FixedUpdate에서 처리하는 것이 유니티의 기본 규칙입니다.
    void FixedUpdate()
    {
        // 대각선 이동 시 속도가 일정하도록 벡터 길이를 1로 정규화(normalized)하여 속도를 곱합니다.
        rb.linearVelocity = movement.normalized * speed;
    }
}

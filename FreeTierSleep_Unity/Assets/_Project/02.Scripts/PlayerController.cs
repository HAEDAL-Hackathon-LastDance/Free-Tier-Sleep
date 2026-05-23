using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    
    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public int maxJumps = 2;
    public float coyoteTime = 0.15f;
    
    [Header("Detection Settings")]
    public LayerMask groundLayer;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public float groundCheckDistance = 0.6f; // 0.1f에서 0.6f로 상향 (캐릭터 절반 높이 0.5f + 여유분 0.1f)

    // 내부 상태 변수
    private Rigidbody2D rb;
    private BoxCollider2D col; // 콜라이더 참조 추가
    private int jumpsRemaining;
    private float coyoteTimer;
    private bool isGrounded;
    private Vector2 moveInput;
    
    // 컴포넌트가 처음 붙거나, 인스펙터에서 Reset을 눌렀을 때 실행됨
    private void Reset()
    {
        groundCheckDistance = 0.6f;
        groundCheckSize = new Vector2(0.8f, 0.1f);
        moveSpeed = 8f;
        jumpForce = 12f;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
        // 초기 설정: 회전 고정 (캐릭터가 넘어지지 않도록)
        rb.freezeRotation = true;
        // 물리 연산 보간 설정 (움직임 부드럽게)
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        CheckGround();
        HandleCoyoteTime();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // --- Input System 메시지 핸들러 ---
    
    // Move 액션 (WASD, 방향키)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    // Jump 액션 (Space)
    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            AttemptJump();
        }
        else
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
        }
    }

    // --- 핵심 로직 ---

    private void CheckGround()
    {
        // 박스캐스트: 캐릭터 중심에서 아래로 발사
        // 거리는 캐릭터 절반 높이보다 약간 더 길어야 바닥에 닿음
        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position, 
            groundCheckSize, 
            0f, 
            Vector2.down, 
            groundCheckDistance, 
            groundLayer
        );

        isGrounded = hit.collider != null;

        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            coyoteTimer = coyoteTime;
        }
    }

    private void HandleCoyoteTime()
    {
        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void AttemptJump()
    {
        Debug.Log($"AttemptJump: isGrounded={isGrounded}, coyoteTimer={coyoteTimer}, jumpsRemaining={jumpsRemaining}");

        if (coyoteTimer > 0 || jumpsRemaining > 0)
        {
            if (!isGrounded && coyoteTimer <= 0)
            {
                jumpsRemaining--;
                Debug.Log("Performed Air Jump");
            }
            else
            {
                jumpsRemaining = maxJumps - 1;
                Debug.Log("Performed Ground Jump");
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimer = 0;
        }
        else
        {
            Debug.LogWarning("Jump Failed: No jumps remaining and not grounded");
        }
    }

    private void ApplyMovement()
    {
        // 좌우 이동 적용 (X축은 입력값, Y축은 물리 엔진의 속도 유지)
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // 에디터에서 지면 체크 범위를 시각적으로 확인하기 위한 함수
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 checkPos = transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireCube(checkPos, groundCheckSize);
    }
}

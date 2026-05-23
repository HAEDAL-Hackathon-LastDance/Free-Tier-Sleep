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
    public float groundCheckDistance = 0.6f;

    // 내부 상태 변수
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private PlayerInput playerInput;
    private InputAction jumpAction;
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
        playerInput = GetComponent<PlayerInput>();
        
        // 입력 액션 직접 참조
        if (playerInput != null)
        {
            jumpAction = playerInput.actions["Jump"];
        }

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        CheckGround();
        HandleCoyoteTime();
        HandleVariableJump();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // --- Input System 메시지 핸들러 ---
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            AttemptJump();
        }
    }

    private void HandleVariableJump()
    {
        // 코드에서 직접 "점프 키가 방금 떼어졌는지" 확인 (에디터 설정 불필요)
        if (jumpAction != null && jumpAction.WasReleasedThisFrame())
        {
            if (rb.linearVelocity.y > 0)
            {
                // 속도를 30%만 남김 (확실한 체감)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.3f);
                Debug.Log("Variable Jump Applied!");
            }
        }
    }

    // --- 핵심 로직 ---

    private void CheckGround()
    {
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
        if (coyoteTimer > 0 || jumpsRemaining > 0)
        {
            if (!isGrounded && coyoteTimer <= 0)
            {
                jumpsRemaining--;
            }
            else
            {
                jumpsRemaining = maxJumps - 1;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            coyoteTimer = 0;
        }
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 checkPos = transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireCube(checkPos, groundCheckSize);
    }
}

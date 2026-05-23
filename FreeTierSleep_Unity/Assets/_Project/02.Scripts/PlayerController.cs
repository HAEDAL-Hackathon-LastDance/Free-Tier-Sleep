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
    public float groundCheckDistance = 0.1f; // 발바닥으로부터 아래로 체크할 거리

    // 내부 상태 변수
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private PlayerInput playerInput;
    private InputAction jumpAction;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private int jumpsRemaining;
    private float coyoteTimer;
    private bool isGrounded;
    private Vector2 moveInput;
    
    // 컴포넌트가 처음 붙거나, 인스펙터에서 Reset을 눌렀을 때 실행됨
    private void Reset()
    {
        groundCheckDistance = 0.1f;
        groundCheckSize = new Vector2(0.8f, 0.1f);
        moveSpeed = 8f;
        jumpForce = 12f;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        playerInput = GetComponent<PlayerInput>();
        
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

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

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            bool isRun = Mathf.Abs(moveInput.x) > 0.1f;
            anim.SetBool("isRun", isRun);
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        if (spriteRenderer != null)
        {
            if (moveInput.x > 0) spriteRenderer.flipX = false;
            else if (moveInput.x < 0) spriteRenderer.flipX = true;
        }
    }

    private void HandleVariableJump()
    {
        if (jumpAction != null && jumpAction.WasReleasedThisFrame())
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.3f);
            }
        }
    }

    private void CheckGround()
    {
        // 발바닥의 정확한 위치 계산 (콜라이더 하단)
        float worldHalfHeight = (col.size.y * transform.localScale.y) * 0.5f;
        Vector2 rayOrigin = (Vector2)transform.position + (col.offset * transform.localScale.y) + Vector2.down * worldHalfHeight;

        // 발바닥에서 아래로 groundCheckDistance만큼만 체크
        RaycastHit2D hit = Physics2D.BoxCast(
            rayOrigin, 
            groundCheckSize, 
            0f, 
            Vector2.down, 
            groundCheckDistance, 
            groundLayer
        );

        isGrounded = hit.collider != null;

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
        {
            jumpsRemaining = maxJumps;
            coyoteTimer = coyoteTime;
        }
    }

    private void HandleCoyoteTime()
    {
        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f) coyoteTimer = 0;
            else coyoteTimer -= Time.deltaTime;
        }
    }

    private void AttemptJump()
    {
        // 1. 코요테 타임 내에 있거나 (지상 점프 인정)
        // 2. 공중 점프 횟수가 남아있는 경우
        if (coyoteTimer > 0 || jumpsRemaining > 0)
        {
            // 실제 공중에서 점프하는 경우
            if (!isGrounded && coyoteTimer <= 0)
            {
                jumpsRemaining--;
                Debug.Log($"Air Jump! Remaining: {jumpsRemaining}");
            }
            else
            {
                // 지상 점프(또는 코요테) 시, 다음 점프를 위해 횟수 1회 사용한 것으로 처리
                jumpsRemaining = maxJumps - 1;
                Debug.Log("Ground Jump!");
            }

            // 점프 힘 적용 (상승 속도 즉시 갱신)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // 점프 직후 코요테 타임 종료
            coyoteTimer = 0;
        }
        else
        {
            Debug.LogWarning("Jump Failed: No jumps remaining");
        }
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.red;
        float worldHalfHeight = (col.size.y * transform.localScale.y) * 0.5f;
        Vector2 rayOrigin = (Vector2)transform.position + (col.offset * transform.localScale.y) + Vector2.down * worldHalfHeight;
        Gizmos.DrawWireCube(rayOrigin + Vector2.down * 0.05f, groundCheckSize);
    }
}

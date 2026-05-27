using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    
    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float fallMultiplier = 2.5f;      // 낙하 시 중력 배수
    public float lowJumpMultiplier = 2f;    // 점프 키를 살짝 눌렀을 때 중력 배수
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
    private bool isDead = false; // 사망 상태 체크
    
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
        if (isDead) return; // 죽었으면 업데이트 중지

        CheckGround();
        HandleCoyoteTime();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead) return; // 죽었으면 물리 이동 중지

        ApplyMovement();
        ApplyBetterJump();
    }

    public void OnMove(InputValue value)
    {
        if (isDead) return;
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isDead) return;
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

    private void ApplyBetterJump()
    {
        if (rb.linearVelocity.y < 0)
        {
            // 떨어질 때 더 빨리 떨어지게 함
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && (jumpAction != null && !jumpAction.IsPressed()))
        {
            // 점프 키를 뗐을 때 상승 속도를 더 빠르게 줄임
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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

            // 점프 힘 적용 (상승 속도 즉시 갱신을 위해 Y속도 초기화 후 AddForce)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
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

    // 외부(InstantKill 등)에서 호출할 수 있는 사망 처리 메서드
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. 색상 어둡게 변경
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
        }

        // 2. 사망 애니메이션 재생 (Animator에 "Die" Trigger가 설정되어 있어야 함)
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 3. 콜라이더 끄기 (바닥을 뚫고 떨어지도록)
        if (col != null)
        {
            col.enabled = false;
        }

        // 4. 위로 살짝 튕겨 오르며 떨어지는 연출
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
        }

        // 5. 입력 컴포넌트 비활성화
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
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

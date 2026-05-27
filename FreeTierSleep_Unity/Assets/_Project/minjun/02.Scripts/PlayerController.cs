using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHp = 3;
    public int currentHp;
    private bool isInvincible = false;
    
    // 체력 변경 시 UI 등에 알리기 위한 이벤트
    public Action<int> OnHealthChanged;

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

    [Header("Glitch Effect Settings")]
    [Tooltip("사망 시 교체될 글리치 머티리얼")]
    public Material glitchMaterial;
    [Tooltip("글리치 셰이더에서 강도를 조절하는 프로퍼티 이름 (예: _Intensity, _Fade 등)")]
    public string glitchPropertyName = "_Intensity";
    [Tooltip("감염 연출이 진행되는 시간")]
    public float glitchDuration = 1.5f;

    // 내부 상태 변수
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private PlayerInput playerInput;
    private InputAction jumpAction;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer glitchOverlayRenderer; // 글리치 효과를 덮어씌울 자식 렌더러
    private int jumpsRemaining;
    private float coyoteTimer;
    private bool isGrounded;
    private Vector2 moveInput;
    
    // 외부(카메라 등)에서 사망 여부를 확인할 수 있도록 public으로 변경
    public bool isDead = false; 
    
    // 컴포넌트가 처음 붙거나, 인스펙터에서 Reset을 눌렀을 때 실행됨
    private void Reset()
    {
        groundCheckDistance = 0.1f;
        groundCheckSize = new Vector2(0.8f, 0.1f);
        moveSpeed = 8f;
        jumpForce = 12f;
        maxHp = 3;
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
        
        currentHp = maxHp;

        // 플레이어 원본 이미지를 유지하면서 위에 글리치를 띄우기 위한 자식 오브젝트 생성
        if (glitchMaterial != null && spriteRenderer != null)
        {
            GameObject overlayObj = new GameObject("GlitchOverlay");
            overlayObj.transform.SetParent(transform);
            overlayObj.transform.localPosition = Vector3.zero;
            overlayObj.transform.localScale = Vector3.one;

            glitchOverlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
            glitchOverlayRenderer.material = glitchMaterial;
            glitchOverlayRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            glitchOverlayRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // 플레이어보다 1단계 앞에 렌더링
            glitchOverlayRenderer.enabled = false; // 평소에는 꺼둠
        }
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

            // 글리치 오버레이가 플레이어의 현재 스프라이트와 방향을 똑같이 따라가도록 동기화
            if (glitchOverlayRenderer != null)
            {
                glitchOverlayRenderer.sprite = spriteRenderer.sprite;
                glitchOverlayRenderer.flipX = spriteRenderer.flipX;
            }
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
            }
            else
            {
                // 지상 점프(또는 코요테) 시, 다음 점프를 위해 횟수 1회 사용한 것으로 처리
                jumpsRemaining = maxJumps - 1;
            }

            // 점프 힘 적용 (상승 속도 즉시 갱신을 위해 Y속도 초기화 후 AddForce)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
            // 점프 직후 코요테 타임 종료
            coyoteTimer = 0;
        }
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // 데미지를 실제로 입었는지 여부를 반환하도록 수정 (이벤트 중복 호출 방지)
    public bool TakeDamage()
    {
        if (isDead || isInvincible) return false;

        currentHp--;
        
        // UI 업데이트 이벤트 호출
        OnHealthChanged?.Invoke(currentHp);

        // 피격 시 글리치 오버레이 켜기 및 오염도 설정
        if (glitchOverlayRenderer != null)
        {
            glitchOverlayRenderer.enabled = true;
            float infectionRatio = 1f - ((float)currentHp / maxHp); // 잃은 체력 비율
            if (glitchOverlayRenderer.material.HasProperty(glitchPropertyName))
            {
                glitchOverlayRenderer.material.SetFloat(glitchPropertyName, infectionRatio);
            }
        }

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
        
        return true;
    }

    // 피격 시 위로 강하게 튕겨 오르게 하여 복구 기회를 제공하는 메서드
    public void BounceUp(float force)
    {
        if (isDead) return;

        // 기존 Y축 속도를 초기화하고 위로 힘을 가함
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        // 공중에서 다시 조작할 수 있도록 점프 횟수 회복
        jumpsRemaining = maxJumps;
    }

    // 무적 상태를 무시하고 즉시 사망 처리하는 메서드 (글리치 심층부 추락 시 사용)
    public void InstantDie()
    {
        if (isDead) return;
        
        currentHp = 0;
        OnHealthChanged?.Invoke(currentHp);
        
        Die();
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float duration = 1f;
        float elapsed = 0f;
        float blinkInterval = 0.1f;

        // 1초간 깜빡임 연출
        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                // 오버레이도 같이 깜빡이도록 동기화
                if (glitchOverlayRenderer != null)
                {
                    glitchOverlayRenderer.enabled = spriteRenderer.enabled;
                }
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // 1초 뒤 무적이 끝나면 글리치 효과 끄기 (원래 플레이어 모습으로 복귀)
        if (glitchOverlayRenderer != null)
        {
            glitchOverlayRenderer.enabled = false;
        }
        
        isInvincible = false;
    }

    // 외부(InstantKill 등)에서 호출할 수 있는 사망 처리 메서드
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // 진행 중인 무적 깜빡임 코루틴 강제 종료
        StopAllCoroutines();

        // 1. 색상 어둡게 변경 및 스프라이트 확실히 켜기
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.gray;
            
            // 사망 시 글리치 오버레이 켜고 감염 연출 시작
            if (glitchOverlayRenderer != null)
            {
                glitchOverlayRenderer.enabled = true;
                StartCoroutine(GlitchInfectionRoutine());
            }
        }

        // 2. 애니메이터 정지
        if (anim != null)
        {
            anim.enabled = false;
        }

        // 3. 콜라이더 끄기
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

    // 점진적으로 감염되는 글리치 연출 코루틴
    private IEnumerator GlitchInfectionRoutine()
    {
        if (glitchOverlayRenderer == null) yield break;

        float startIntensity = 0f;
        if (glitchOverlayRenderer.material.HasProperty(glitchPropertyName))
        {
            startIntensity = glitchOverlayRenderer.material.GetFloat(glitchPropertyName);
        }

        float elapsed = 0f;
        
        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glitchDuration;
            
            // 현재 오염도에서 1(최대치)까지 서서히 증가시킴
            if (glitchOverlayRenderer.material.HasProperty(glitchPropertyName))
            {
                glitchOverlayRenderer.material.SetFloat(glitchPropertyName, Mathf.Lerp(startIntensity, 1f, t));
            }
            
            yield return null;
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

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
    public bool IsInvincible => isInvincible;
    
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

    [Header("Camera Bounds")]
    [Tooltip("플레이어가 카메라 좌우 경계를 벗어나지 못하도록 X축을 클램핑")]
    public bool clampToCameraBounds = true;
    [Tooltip("카메라 가장자리로부터 추가로 안쪽으로 들여놓을 여백 (월드 단위)")]
    public float cameraBoundsPadding = 0.2f;

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
    private Camera mainCamera; // 카메라 클램핑용
    private float cachedHalfCamWidth; // orthographicSize * aspect는 거의 변하지 않으므로 Start에서 캐싱
    private int jumpsRemaining;
    private float coyoteTimer;
    private bool isGrounded;
    private Vector2 moveInput;
    
    // 외부(카메라 등)에서 사망 여부를 확인할 수 있도록 public으로 변경
    public bool isDead = false;

    // [상태 이상] 조작 반전 / 중력 변조 상태
    private bool isInputReversed = false;
    private Coroutine inputReverseCoroutine;
    private Coroutine gravityDebuffCoroutine;
    private float defaultGravityScale; // 인스펙터의 원본 중력 값을 보존

    // BounceUp 직후 일정 시간 동안 lowJumpMultiplier 적용 안 함 (튕긴 후 상승이 일찍 죽지 않도록)
    private float bounceBoostUntil;
    private Coroutine invincibilityCoroutine; // 무적 코루틴 참조 (중복 방지 및 재설정용)
    private float knockbackUntil; // 이 시간까지 수평 이동 입력을 무시 (넉백 직후 다시 밀려 들어가는 것 방지)

    [Header("Bounce Settings")]
    [Tooltip("BounceUp 호출 후 이 시간 동안 점프 키 안 눌러도 상승 감속(lowJumpMultiplier)을 적용하지 않음")]
    public float bounceBoostDuration = 0.3f;
    [Tooltip("바운스 후 무적 지속 시간 (초) — 연속 피해를 막아 회복 기회를 보장")]
    public float bounceInvincibleDuration = 2f;

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

        // 원본 중력 값 저장 (디버프 회복 기준점)
        defaultGravityScale = rb.gravityScale;

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cachedHalfCamWidth = mainCamera.orthographicSize * mainCamera.aspect;
        }

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

    // 카메라가 플레이어를 따라간 직후 X 위치를 카메라 좌우 경계로 가둠
    private void LateUpdate()
    {
        if (isDead || !clampToCameraBounds || mainCamera == null || col == null) return;

        // 콜라이더 절반 폭 (월드 스케일 반영) + 추가 여백
        float halfPlayerW = (col.size.x * Mathf.Abs(transform.localScale.x)) * 0.5f;
        float minX = mainCamera.transform.position.x - cachedHalfCamWidth + halfPlayerW + cameraBoundsPadding;
        float maxX = mainCamera.transform.position.x + cachedHalfCamWidth - halfPlayerW - cameraBoundsPadding;

        if (transform.position.x < minX || transform.position.x > maxX)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            transform.position = pos;
            // 경계에 부딪힌 순간 X속도 제거 → 경계에 박혀 부르르 떨리는 현상 방지
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
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
        else if (rb.linearVelocity.y > 0 && (jumpAction != null && !jumpAction.IsPressed()) && Time.time >= bounceBoostUntil)
        {
            // 점프 키를 뗐을 때 상승 속도를 더 빠르게 줄임 (단, BounceUp 직후 보호 시간 동안은 적용 안 함)
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

        isGrounded = false;

        if (hit.collider != null)
        {
            // 플레이어가 위로 상승 중일 때는 무조건 Ground 판정 제외 (원웨이 플랫폼 통과 시)
            if (rb.linearVelocity.y > 0.1f)
            {
                isGrounded = false;
            }
            else
            {
                PlatformEffector2D effector = hit.collider.GetComponent<PlatformEffector2D>();
                if (effector != null && effector.useOneWay)
                {
                    // 원웨이 플랫폼일 경우, 플레이어 발바닥이 플랫폼 윗면보다 높을 때만 인정
                    float playerBottom = col.bounds.min.y;
                    float platformTop = hit.collider.bounds.max.y;
                    
                    if (playerBottom >= platformTop - 0.15f)
                    {
                        isGrounded = true;
                    }
                }
                else
                {
                    isGrounded = true;
                }
            }
        }

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
        // 넉백 직후에는 수평 이동 입력을 무시해 다시 적에게 밀려 들어가지 않도록 함
        if (Time.time < knockbackUntil) return;

        // 조작 반전 상태이면 입력 X축에 -1을 곱함 (혼란 팝업 효과)
        float effectiveX = isInputReversed ? -moveInput.x : moveInput.x;
        rb.linearVelocity = new Vector2(effectiveX * moveSpeed, rb.linearVelocity.y);
    }

    // 넉백 직후 수평 이동 입력 차단 (Knocker 등 외부 강제 이동 시 사용)
    public void ApplyExternalKnockback(float duration)
    {
        knockbackUntil = Time.time + duration;
    }

    // [상태 이상] 조작 반전 - 일정 시간 동안 좌우 입력 뒤집기 (혼란 팝업)
    public void ApplyInputReverse(float duration)
    {
        if (isDead) return;

        if (inputReverseCoroutine != null) StopCoroutine(inputReverseCoroutine);
        inputReverseCoroutine = StartCoroutine(InputReverseRoutine(duration));
    }

    private IEnumerator InputReverseRoutine(float duration)
    {
        isInputReversed = true;
        yield return new WaitForSeconds(duration);
        isInputReversed = false;
        inputReverseCoroutine = null;
    }

    // [상태 이상] 중력 변조 - 일정 시간 동안 중력 배수 변경 (무게 텍스트)
    public void ApplyGravityDebuff(float duration, float multiplier = 2f)
    {
        if (isDead) return;

        // 중복 적용 방지: 진행 중이던 디버프를 취소하고 원본 중력으로 복귀시킨 뒤 새로 시작
        if (gravityDebuffCoroutine != null)
        {
            StopCoroutine(gravityDebuffCoroutine);
            rb.gravityScale = defaultGravityScale;
        }
        gravityDebuffCoroutine = StartCoroutine(GravityDebuffRoutine(duration, multiplier));
    }

    private IEnumerator GravityDebuffRoutine(float duration, float multiplier)
    {
        rb.gravityScale = defaultGravityScale * multiplier;
        yield return new WaitForSeconds(duration);
        rb.gravityScale = defaultGravityScale;
        gravityDebuffCoroutine = null;
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

        Phase1AudioManager.Instance?.PlayGlitch();

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            StartInvincibility(1f);
        }
        
        return true;
    }

    // 피격 시 위로 강하게 튕겨 오르게 하여 복구 기회를 제공하는 메서드
    public void BounceUp(float force)
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        jumpsRemaining = maxJumps;
        bounceBoostUntil = Time.time + bounceBoostDuration;
        StartInvincibility(bounceInvincibleDuration);
    }

    // 낙하 속도에 비례한 적응형 바운스 — 가비지 콜렉터 충돌 시 사용
    // 빠르게 떨어질수록 더 높이 튕겨 오르며, min/max로 상하한 보장
    public void BounceUpAdaptive(float minSpeed = 30f, float maxSpeed = 75f)
    {
        if (isDead) return;

        float fallSpeed = Mathf.Abs(Mathf.Min(rb.linearVelocity.y, 0f));
        float bounceSpeed = Mathf.Clamp(fallSpeed, minSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceSpeed);
        jumpsRemaining = maxJumps;
        bounceBoostUntil = Time.time + bounceBoostDuration;
        StartInvincibility(bounceInvincibleDuration);
    }

    // 무적 코루틴 시작 — 이미 진행 중이면 더 긴 쪽으로 교체
    private void StartInvincibility(float duration)
    {
        if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
        invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
    }

    // 하트 아이템 — 체력 1칸 회복 (최대치 초과 금지)
    public void HealOne()
    {
        if (isDead || currentHp >= maxHp) return;

        currentHp++;
        OnHealthChanged?.Invoke(currentHp);

        // 회복으로 글리치 오염도 감소
        if (glitchOverlayRenderer != null)
        {
            float infectionRatio = 1f - ((float)currentHp / maxHp);
            if (glitchOverlayRenderer.material.HasProperty(glitchPropertyName))
                glitchOverlayRenderer.material.SetFloat(glitchPropertyName, infectionRatio);
            if (currentHp >= maxHp) glitchOverlayRenderer.enabled = false;
        }
    }

    // 슈퍼점프 스프링 — 매우 높이 튀어 오르며 일정 시간 무적
    public void SuperJump(float force, float invincibleDuration)
    {
        if (isDead) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        jumpsRemaining = maxJumps;
        bounceBoostUntil = Time.time + bounceBoostDuration;

        // 가비지 콜렉터 파도와 충돌 시와 동일한 글리치 오염 효과
        if (glitchOverlayRenderer != null)
        {
            glitchOverlayRenderer.enabled = true;
            if (glitchOverlayRenderer.material.HasProperty(glitchPropertyName))
                glitchOverlayRenderer.material.SetFloat(glitchPropertyName, 1f);
        }

        StartInvincibility(invincibleDuration);
    }

    // 무적 상태를 무시하고 즉시 사망 처리하는 메서드 (글리치 심층부 추락 시 사용)
    public void InstantDie()
    {
        if (isDead) return;
        
        currentHp = 0;
        OnHealthChanged?.Invoke(currentHp);
        
        Die();
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;
        float blinkInterval = 0.1f;

        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                if (glitchOverlayRenderer != null)
                    glitchOverlayRenderer.enabled = spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (glitchOverlayRenderer != null) glitchOverlayRenderer.enabled = false;

        isInvincible = false;
        invincibilityCoroutine = null;
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

        // 6. 글리치 연출 후 게임오버 팝업
        StartCoroutine(TriggerGameOver());
    }

    private IEnumerator TriggerGameOver()
    {
        yield return new WaitForSeconds(glitchDuration + 0.2f);
        Time.timeScale = 0f;
        GameOverScreen.Instance?.Show(transform.position.y);
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
        if (col == null) return;

        Gizmos.color = Color.red;
        float worldHalfHeight = (col.size.y * transform.localScale.y) * 0.5f;
        Vector2 rayOrigin = (Vector2)transform.position + (col.offset * transform.localScale.y) + Vector2.down * worldHalfHeight;
        Gizmos.DrawWireCube(rayOrigin + Vector2.down * groundCheckDistance * 0.5f, groundCheckSize);
    }
}

using UnityEngine;

public class RisingDataFlood : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("파도의 초기 상승 속도")]
    public float riseSpeed = 2f;

    [Tooltip("파도가 도달할 수 있는 최대 Y 좌표")]
    public float maxYPosition = 50f;

    [Header("Step Acceleration Settings")]
    [Tooltip("1단계 가속 트리거 고도 (플레이어 Y 기준)")]
    public float step1Altitude = 300f;

    [Tooltip("2단계 가속 트리거 고도 (플레이어 Y 기준)")]
    public float step2Altitude = 600f;

    [Tooltip("1단계 가속 배율 (기획: 1.1배)")]
    public float step1Multiplier = 1.1f;

    [Tooltip("2단계 가속 배율 (기획: 1.2배, 기본 속도 기준 누적 적용)")]
    public float step2Multiplier = 1.2f;

    [Header("Catch-up Settings")]
    [Tooltip("플레이어와 파도 상단 사이 허용 거리. 이보다 멀어지면 파도가 점점 빠르게 따라옴 (낙하 거리 폭주 방지)")]
    public float maxDistanceFromPlayer = 25f;

    [Tooltip("거리가 멀어졌을 때 기본 riseSpeed에 추가되는 최대 속도 (유닛/초)")]
    public float catchUpBonusSpeed = 12f;

    [Tooltip("허용 거리 초과 1유닛당 추가되는 catch-up 속도 가중치")]
    public float catchUpPerUnit = 0.5f;

    private float baseRiseSpeed;    // 초기 속도를 보존하여 배율 계산 기준으로 사용
    private bool step1Applied = false;
    private bool step2Applied = false;

    private bool isMoving = true;
    private Camera mainCamera;
    private PlayerController player;
    private Collider2D col;

    // 외부(LevelGenerator 등)에서 현재 파도의 Y 위치를 가져갈 수 있도록 프로퍼티 추가
    public float CurrentY => transform.position.y;

    private void Start()
    {
        mainCamera = Camera.main;
        player = Object.FindFirstObjectByType<PlayerController>();
        col = GetComponent<Collider2D>();
        baseRiseSpeed = riseSpeed;
    }

    void LateUpdate()
    {
        if (!isMoving) return;

        // 플레이어가 살아있을 때만 단계별 가속 체크
        if (player != null && !player.isDead)
        {
            float playerY = player.transform.position.y;

            // Y=300 돌파 시 1.1배 가속 (1회만 적용)
            if (!step1Applied && playerY >= step1Altitude)
            {
                riseSpeed = baseRiseSpeed * step1Multiplier;
                step1Applied = true;
            }

            // Y=600 돌파 시 1.2배 가속 (기본 속도 기준, 1회만 적용)
            if (!step2Applied && playerY >= step2Altitude)
            {
                riseSpeed = baseRiseSpeed * step2Multiplier;
                step2Applied = true;
            }
        }

        // 플레이어가 사망한 경우, 글리치의 바닥이 카메라의 바닥까지만 올라오도록 제한
        if (player != null && player.isDead && mainCamera != null && col != null)
        {
            // 해상도나 카메라 모드(원근/직교)에 상관없이 화면 맨 아래(Viewport Y: 0)의 정확한 월드 좌표를 구함
            float zDistance = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
            float cameraBottomY = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance)).y;
            
            float glitchBottomY = col.bounds.min.y;
            float currentY = transform.position.y;
            
            // 오브젝트의 중심(Y)과 바닥(min.y) 사이의 거리(오프셋) 계산
            float bottomOffset = currentY - glitchBottomY;
            
            // 글리치가 도달해야 할 최종 목표 Y 좌표
            float targetY = cameraBottomY + bottomOffset;

            // 글리치 바닥이 이미 카메라 바닥보다 높거나, 이번 프레임 이동 시 넘어서는 경우
            if (currentY + (riseSpeed * Time.deltaTime) >= targetY || currentY >= targetY)
            {
                // Translate 대신 position을 직접 설정하여 오차 없이 완벽하게 스냅
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                return;
            }
        }

        // catch-up: 플레이어가 너무 높이 올라간 경우 파도가 점점 빠르게 따라옴
        // 평소엔 기본 riseSpeed, 거리가 maxDistanceFromPlayer를 넘기 시작하면 거리 비례 가산
        float effectiveRiseSpeed = riseSpeed;
        if (player != null && !player.isDead && col != null)
        {
            float distance = player.transform.position.y - col.bounds.max.y;
            if (distance > maxDistanceFromPlayer)
            {
                float overDistance = distance - maxDistanceFromPlayer;
                effectiveRiseSpeed += Mathf.Min(overDistance * catchUpPerUnit, catchUpBonusSpeed);
            }
        }

        // 매 프레임 위로 이동 (물리적인 오브젝트의 이동)
        transform.position += Vector3.up * effectiveRiseSpeed * Time.deltaTime;

        // 최대 높이에 도달하면 이동 정지
        if (transform.position.y >= maxYPosition)
        {
            isMoving = false;
            Vector3 pos = transform.position;
            pos.y = maxYPosition;
            transform.position = pos;
        }
    }

    // 외부에서 파도를 멈추게 할 때 사용할 수 있는 메서드
    public void StopFlood()
    {
        isMoving = false;
    }
}

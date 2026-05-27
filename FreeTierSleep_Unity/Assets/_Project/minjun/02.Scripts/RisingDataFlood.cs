using UnityEngine;

public class RisingDataFlood : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("파도가 초당 위로 올라가는 속도")]
    public float riseSpeed = 2f;
    
    [Tooltip("파도가 도달할 수 있는 최대 Y 좌표")]
    public float maxYPosition = 50f;

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
    }

    void LateUpdate()
    {
        if (!isMoving) return;

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

        // 매 프레임 위로 이동 (물리적인 오브젝트의 이동)
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

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

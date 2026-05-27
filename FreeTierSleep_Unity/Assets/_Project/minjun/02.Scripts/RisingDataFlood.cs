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

    void Update()
    {
        if (!isMoving) return;

        // 플레이어가 사망한 경우, 글리치의 바닥이 카메라의 바닥까지만 올라오도록 제한
        if (player != null && player.isDead && mainCamera != null && col != null)
        {
            float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;
            float glitchBottomY = col.bounds.min.y;
            float moveStep = riseSpeed * Time.deltaTime;

            // 글리치 바닥이 이미 카메라 바닥보다 높거나(카메라가 추락하는 플레이어를 따라 내려온 경우),
            // 이번 프레임에 이동할 거리가 카메라 바닥을 넘어서는 경우
            if (glitchBottomY + moveStep >= cameraBottomY)
            {
                // 글리치의 바닥을 카메라의 바닥에 정확히 맞춤 (오차 및 여백 완벽 보정)
                float distanceToMove = cameraBottomY - glitchBottomY;
                transform.Translate(Vector3.up * distanceToMove);
                return;
            }
        }

        // 매 프레임 위로 이동 (물리적인 오브젝트의 이동)
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);

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

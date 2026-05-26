using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("따라갈 플레이어 오브젝트를 연결하세요.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("카메라가 따라가는 속도 (값이 클수록 빠름)")]
    public float smoothSpeed = 5f;
    
    [Tooltip("화면 내 플레이어의 Y축 오프셋 (양수면 플레이어가 화면 중앙보다 아래에 위치)")]
    public float yOffset = 2f;

    // 카메라가 도달한 최고 높이를 저장 (아래로 내려가지 않게 하기 위함)
    private float highestY;

    void Start()
    {
        // 시작할 때 카메라의 현재 Y 위치를 최고 높이로 초기화
        highestY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어의 현재 위치에 오프셋을 더한 목표 Y 좌표 계산
        float targetY = target.position.y + yOffset;

        // 기획서 규칙: 카메라는 아래로 절대 내려가지 않음
        // 목표 Y 좌표가 지금까지의 최고 높이보다 높을 때만 갱신
        if (targetY > highestY)
        {
            highestY = targetY;
        }

        // 현재 카메라 위치에서 목표 높이(highestY)까지 부드럽게 보간(Lerp) 이동
        float newY = Mathf.Lerp(transform.position.y, highestY, smoothSpeed * Time.deltaTime);

        // 카메라의 X, Z 위치는 그대로 유지하고 Y 위치만 업데이트
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

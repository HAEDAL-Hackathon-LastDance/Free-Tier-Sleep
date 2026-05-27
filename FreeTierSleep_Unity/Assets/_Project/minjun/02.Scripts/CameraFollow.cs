using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("따라갈 플레이어 오브젝트를 연결하세요. 비워두면 Player 태그를 가진 오브젝트를 자동 탐색합니다.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("카메라가 따라가는 속도 (값이 클수록 빠름)")]
    public float smoothSpeed = 5f;
    
    [Tooltip("화면 내 플레이어의 Y축 오프셋 (양수면 플레이어가 화면 중앙보다 아래에 위치)")]
    public float yOffset = 2f;

    void Start()
    {
        // 타겟이 지정되지 않았다면 Player 태그를 가진 오브젝트를 자동으로 찾습니다.
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: 씬에서 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            }
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 플레이어가 사망했는지 확인
        PlayerController player = target.GetComponent<PlayerController>();
        if (player != null && player.isDead)
        {
            // 플레이어가 죽었다면 카메라 이동을 멈추고 현재 위치 유지
            return;
        }

        // 플레이어의 현재 위치에 오프셋을 더한 목표 Y 좌표 계산 (위아래 모두 추적)
        float targetY = target.position.y + yOffset;

        // 현재 카메라 위치에서 목표 높이(targetY)까지 부드럽게 보간(Lerp) 이동
        float newY = Mathf.Lerp(transform.position.y, targetY, smoothSpeed * Time.deltaTime);

        // 카메라의 X, Z 위치는 그대로 유지하고 Y 위치만 업데이트
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

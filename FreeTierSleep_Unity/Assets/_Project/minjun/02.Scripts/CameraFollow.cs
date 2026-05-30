using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("따라갈 플레이어 오브젝트를 연결하세요. 비워두면 Player 태그를 가진 오브젝트를 자동 탐색합니다.")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("카메라가 따라가는 기본 속도 (값이 클수록 빠름)")]
    public float smoothSpeed = 5f;

    [Tooltip("화면 내 플레이어의 Y축 오프셋 (양수면 플레이어가 화면 중앙보다 아래에 위치)")]
    public float yOffset = 2f;

    [Header("Catch-up Settings")]
    [Tooltip("플레이어와 카메라가 벌어진 거리에 비례해 추적 속도를 가산 (0이면 비활성, 빠른 낙하 시 카메라가 못 따라가는 문제 보정)")]
    public float catchUpFactor = 2.5f;

    [Tooltip("이 거리 이상 벌어지면 즉시 따라잡음 (카메라 시야 절반보다 작게 설정해야 플레이어가 항상 화면 안)")]
    public float maxFollowDistance = 7f;

    private PlayerController cachedPlayer; // 매 프레임 GetComponent 방지

    void Start()
    {
        // 타겟이 지정되지 않았다면 Player 태그를 가진 오브젝트를 자동으로 찾음
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
            else Debug.LogWarning("CameraFollow: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        if (target != null) cachedPlayer = target.GetComponent<PlayerController>();
    }

    void LateUpdate()
    {
        if (target == null) return;
        if (cachedPlayer != null && cachedPlayer.isDead) return; // 사망 시 카메라 정지

        float targetY = target.position.y + yOffset;
        float distance = Mathf.Abs(targetY - transform.position.y);

        float newY;
        if (distance > maxFollowDistance)
        {
            // 너무 멀어졌다 → 임계 거리까지 즉시 스냅 (충돌 순간이 화면 밖에서 발생하지 않도록)
            float sign = Mathf.Sign(targetY - transform.position.y);
            newY = targetY - sign * maxFollowDistance;
        }
        else
        {
            // 평소엔 기본 lerp, 거리가 벌어질수록 속도 가산해 catch-up
            float adaptiveSpeed = smoothSpeed + distance * catchUpFactor;
            newY = Mathf.Lerp(transform.position.y, targetY, adaptiveSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

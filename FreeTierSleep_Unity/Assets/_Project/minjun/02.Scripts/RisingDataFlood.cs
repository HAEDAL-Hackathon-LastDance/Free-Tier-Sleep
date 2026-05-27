using UnityEngine;

public class RisingDataFlood : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("파도가 초당 위로 올라가는 속도")]
    public float riseSpeed = 2f;
    
    [Tooltip("파도가 도달할 수 있는 최대 Y 좌표")]
    public float maxYPosition = 50f;

    private bool isMoving = true;

    void Update()
    {
        if (!isMoving) return;

        // 매 프레임 위로 이동
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

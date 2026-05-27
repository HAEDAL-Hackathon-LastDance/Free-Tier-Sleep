using UnityEngine;

public class RisingDataFlood : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("파도가 초당 위로 올라가는 속도")]
    public float riseSpeed = 2f;

    void Update()
    {
        // 매 프레임 위로 이동
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
    }
}

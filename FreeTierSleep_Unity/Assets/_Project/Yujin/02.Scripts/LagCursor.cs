using UnityEngine;

public class LagCursor : MonoBehaviour
{
    public static LagCursor Instance { get; private set; }

    public Vector3 WorldPosition { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        WorldPosition = transform.position;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 target = Camera.main.ScreenToWorldPoint(mousePos);
        target.z = 0f;

        float speed = LoadManager.Instance != null ? LoadManager.Instance.CurrentLerpSpeed : 0.5f;
        WorldPosition = Vector3.Lerp(WorldPosition, target, speed);
        transform.position = WorldPosition;
    }
}

using UnityEngine;

public class LoadManager : MonoBehaviour
{
    public static LoadManager Instance { get; private set; }

    [SerializeField] private float minLerpSpeed = 0.05f;
    [SerializeField] private float maxLerpSpeed = 0.5f;
    [SerializeField] private int maxEnemyCount = 20;

    public float CurrentLerpSpeed { get; private set; } = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float load = Mathf.Clamp01((float)enemies.Length / maxEnemyCount);
        CurrentLerpSpeed = Mathf.Lerp(maxLerpSpeed, minLerpSpeed, load);
    }
}

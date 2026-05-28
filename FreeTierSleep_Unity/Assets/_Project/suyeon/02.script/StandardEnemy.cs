using UnityEngine;

public class StandardEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 1.0f;
    private float attackTimer = 100f;

    [Header("Enemy Settings")]
    public float moveSpeed = 1.0f;
    public int attackDamage = 10;

    [Header("Target Tracking")]
    public Transform coreTransform;

    void Start()
    {
        GameObject realPlayer = GameObject.FindGameObjectWithTag("Player");
        if (realPlayer != null)
            coreTransform = realPlayer.transform;
    }

    void Update()
    {
        if (coreTransform != null)
            transform.position = Vector2.MoveTowards(transform.position, coreTransform.position, moveSpeed * Time.deltaTime);

        attackTimer += Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (attackTimer >= attackCooldown)
            {
                PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(attackDamage);

                    AudioSource playerAudio = other.GetComponent<AudioSource>();
                    if (playerAudio != null)
                        playerAudio.Play();

                    attackTimer = 0f;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 방화벽 선(Stroke)에 닿으면 즉시 소멸
        if (other.GetComponent<Stroke>() != null)
            Destroy(gameObject);
    }
}
